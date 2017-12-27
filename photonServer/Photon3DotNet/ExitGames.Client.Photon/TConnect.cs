namespace ExitGames.Client.Photon
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Security;
    using System.Threading;

    /// <summary>
    /// Internal class to encapsulate the network i/o functionality for the realtime libary. 
    /// </summary>
    internal class TConnect
    {
        private const int ALL_HEADER_BYTES = 9;

        internal bool isRunning;

        private const int MSG_HEADER_BYTES = 2;

        internal bool obsolete;

        internal TPeer peer;

        private EndPoint serverEndPoint;

        private Socket socketConnection;

        internal const int TCP_HEADER_BYTES = 7;

        internal TConnect(TPeer npeer, string ipPort)
        {
            if (npeer.debugOut >= DebugLevel.ALL)
            {
                npeer.Listener.DebugReturn(DebugLevel.ALL, "new TConnect()");
            }
            this.peer = npeer;
        }

        public void Run()
        {
            try
            {
                this.serverEndPoint = PeerBase.GetEndpoint(this.peer.ServerAddress);
                if (this.serverEndPoint == null)
                {
                    if (this.peer.debugOut >= DebugLevel.ERROR)
                    {
                        this.peer.Listener.DebugReturn(DebugLevel.ERROR, "StartConnection() failed. Address must be 'address:port'. Is: " + this.peer.ServerAddress);
                    }
                    return;
                }
                this.socketConnection.Connect(this.serverEndPoint);
            }
            catch (SecurityException se)
            {
                if (this.peer.debugOut >= DebugLevel.INFO)
                {
                    this.peer.Listener.DebugReturn(DebugLevel.INFO, "Connect() failed: " + se.ToString());
                }
                if (this.socketConnection != null)
                {
                    this.socketConnection.Close();
                }
                this.isRunning = false;
                this.obsolete = true;
                this.peer.EnqueueStatusCallback(StatusCode.ExceptionOnConnect);
                this.peer.EnqueueActionForDispatch(delegate
                {
                    this.peer.Disconnected();
                });
                return;
            }
            catch (SocketException se)
            {
                if (this.peer.debugOut >= DebugLevel.INFO)
                {
                    this.peer.Listener.DebugReturn(DebugLevel.INFO, "Connect() failed: " + se.ToString());
                }
                if (this.socketConnection != null)
                {
                    this.socketConnection.Close();
                }
                this.isRunning = false;
                this.obsolete = true;
                this.peer.EnqueueStatusCallback(StatusCode.ExceptionOnConnect);
                this.peer.EnqueueActionForDispatch(delegate
                {
                    this.peer.Disconnected();
                });
                return;
            }
            this.obsolete = false;
            this.isRunning = true;
            while (!this.obsolete)
            {
                MemoryStream opCollectionStream = new MemoryStream(0x100);
                try
                {
                    int bytesRead = 0;
                    byte[] inBuff = new byte[9];
                    while (bytesRead < 9)
                    {
                        bytesRead += this.socketConnection.Receive(inBuff, bytesRead, 9 - bytesRead, SocketFlags.None);
                        if (bytesRead == 0)
                        {
                            this.peer.SendPing();
                            Thread.Sleep(100);
                        }
                    }
                    if (inBuff[0] == 240)
                    {
                        if (this.peer.TrafficStatsEnabled)
                        {
                            this.peer.TrafficStatsIncoming.CountControlCommand(inBuff.Length);
                        }
                        if (this.peer.NetworkSimulationSettings.IsSimulationEnabled)
                        {
                            this.peer.ReceiveNetworkSimulated(delegate
                            {
                                this.peer.ReceiveIncomingCommands(inBuff, inBuff.Length);
                            });
                        }
                        else
                        {
                            this.peer.ReceiveIncomingCommands(inBuff, inBuff.Length);
                        }
                        continue;
                    }
                    int length = (((inBuff[1] << 0x18) | (inBuff[2] << 0x10)) | (inBuff[3] << 8)) | inBuff[4];
                    if (this.peer.TrafficStatsEnabled)
                    {
                        if (inBuff[5] == 0)
                        {
                            this.peer.TrafficStatsIncoming.CountReliableOpCommand(length);
                        }
                        else
                        {
                            this.peer.TrafficStatsIncoming.CountUnreliableOpCommand(length);
                        }
                    }
                    if (this.peer.debugOut >= DebugLevel.ALL)
                    {
                        this.peer.EnqueueDebugReturn(DebugLevel.ALL, "message length: " + length);
                    }
                    opCollectionStream.Write(inBuff, 7, bytesRead - 7);
                    bytesRead = 0;
                    length -= 9;
                    inBuff = new byte[length];
                    while (bytesRead < length)
                    {
                        bytesRead += this.socketConnection.Receive(inBuff, bytesRead, length - bytesRead, SocketFlags.None);
                    }
                    opCollectionStream.Write(inBuff, 0, bytesRead);
                    if (opCollectionStream.Length > 0L)
                    {
                        if (this.peer.NetworkSimulationSettings.IsSimulationEnabled)
                        {
                            this.peer.ReceiveNetworkSimulated(delegate
                            {
                                this.peer.ReceiveIncomingCommands(opCollectionStream.ToArray(), (int)opCollectionStream.Length);
                            });
                        }
                        else
                        {
                            this.peer.ReceiveIncomingCommands(opCollectionStream.ToArray(), (int)opCollectionStream.Length);
                        }
                    }
                    if (this.peer.debugOut >= DebugLevel.ALL)
                    {
                        this.peer.EnqueueDebugReturn(DebugLevel.ALL, "TCP < " + opCollectionStream.Length);
                    }
                }
                catch (SocketException e)
                {
                    if (!this.obsolete)
                    {
                        this.obsolete = true;
                        if (this.peer.debugOut >= DebugLevel.ERROR)
                        {
                            this.peer.EnqueueDebugReturn(DebugLevel.ERROR, "Receiving failed. SocketException: " + e.SocketErrorCode);
                        }
                        switch (e.SocketErrorCode)
                        {
                            case SocketError.ConnectionAborted:
                            case SocketError.ConnectionReset:
                                this.peer.EnqueueStatusCallback(StatusCode.DisconnectByServer);
                                continue;
                        }
                        this.peer.EnqueueStatusCallback(StatusCode.Exception);
                    }
                }
                catch (Exception e)
                {
                    if (!this.obsolete && (this.peer.debugOut >= DebugLevel.ERROR))
                    {
                        this.peer.EnqueueDebugReturn(DebugLevel.ERROR, "Receiving failed. Exception: " + e.ToString());
                    }
                }
            }
            if (this.socketConnection != null)
            {
                this.socketConnection.Close();
            }
            this.isRunning = false;
            this.obsolete = true;
            this.peer.EnqueueActionForDispatch(delegate
            {
                this.peer.Disconnected();
            });
        }

        /// <summary>
        /// used by TPeer*
        /// </summary>
        /// <param name="opData">The op Data.</param>
        public void sendTcp(byte[] opData)
        {
            if (this.obsolete)
            {
                if (this.peer.debugOut >= DebugLevel.INFO)
                {
                    this.peer.Listener.DebugReturn(DebugLevel.INFO, "Sending was skipped because connection is obsolete. " + Environment.StackTrace);
                }
            }
            else
            {
                try
                {
                    this.socketConnection.Send(opData);
                }
                catch (NullReferenceException e)
                {
                    if (this.peer.debugOut >= DebugLevel.ERROR)
                    {
                        this.peer.Listener.DebugReturn(DebugLevel.ERROR, e.Message);
                    }
                }
                catch (SocketException e)
                {
                    if (this.peer.debugOut >= DebugLevel.ERROR)
                    {
                        this.peer.Listener.DebugReturn(DebugLevel.ERROR, e.Message);
                    }
                }
            }
        }

        internal bool StartConnection()
        {
            if (this.isRunning)
            {
                if (this.peer.debugOut >= DebugLevel.ERROR)
                {
                    this.peer.Listener.DebugReturn(DebugLevel.ERROR, "startConnectionThread() failed: connection thread still running.");
                }
                return false;
            }
            this.socketConnection = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            this.socketConnection.NoDelay = true;
            new Thread(new ThreadStart(this.Run)).Start();
            return true;
        }

        internal void StopConnection()
        {
            if (this.peer.debugOut >= DebugLevel.ALL)
            {
                this.peer.Listener.DebugReturn(DebugLevel.ALL, "StopConnection()");
            }
            this.obsolete = true;
            if (this.socketConnection != null)
            {
                this.socketConnection.Close();
            }
        }
    }
}
