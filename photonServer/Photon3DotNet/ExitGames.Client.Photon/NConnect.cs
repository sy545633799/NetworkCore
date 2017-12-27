namespace ExitGames.Client.Photon
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Security;

    /// <summary>
    /// Internal class to encapsulate the network i/o functionality for the realtime libary.
    /// </summary>
    internal class NConnect
    {
        internal bool isRunning;
        internal bool obsolete;
        internal EnetPeer peer;
        private EndPoint serverEndPoint;
        private Socket sock;
        private object syncer = new object();

        internal NConnect(EnetPeer npeer)
        {
            if (npeer.debugOut >= DebugLevel.ALL)
            {
                npeer.Listener.DebugReturn(DebugLevel.ALL, "NConnect().");
            }
            this.peer = npeer;
            this.sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        }

        private void HandleSocketErrrors(SocketAsyncEventArgs socketEventArgs)
        {
            if (this.peer.debugOut >= DebugLevel.ERROR && !this.obsolete)
            {
                this.peer.EnqueueDebugReturn(DebugLevel.ERROR, string.Format("{1} failed with SocketError: {0}. obsolete={2}", socketEventArgs.SocketError, socketEventArgs.LastOperation, this.obsolete));
            }
            if (socketEventArgs.LastOperation == SocketAsyncOperation.Connect)
            {
                this.obsolete = true;
                this.peer.EnqueueStatusCallback(StatusCode.ExceptionOnConnect);
                this.peer.EnqueueActionForDispatch(delegate
                {
                    this.peer.Disconnected();
                });
            }
            else if (socketEventArgs.SocketError != SocketError.Shutdown)
            {
                if (socketEventArgs.SocketError == SocketError.ConnectionReset)
                {
                    this.peer.EnqueueStatusCallback(StatusCode.Exception);
                    this.peer.EnqueueActionForDispatch(delegate
                    {
                        this.StopConnection();
                    });
                }
                else if (socketEventArgs.SocketError == SocketError.OperationAborted)
                {
                    if (!this.obsolete)
                    {
                        this.obsolete = true;
                        this.peer.EnqueueActionForDispatch(delegate
                        {
                            this.peer.Disconnected();
                        });
                    }
                }
                else if (this.peer.debugOut >= DebugLevel.ERROR)
                {
                    this.peer.EnqueueDebugReturn(DebugLevel.ERROR, "ProcessReceive() unhandled socket event: " + socketEventArgs.SocketError);
                }
            }
        }

        private void ProcessConnect(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            if (this.peer.debugOut >= DebugLevel.ALL)
            {
                this.peer.EnqueueDebugReturn(DebugLevel.ALL, "ProcessConnect() " + socketAsyncEventArgs.SocketError);
            }
            NCommand command = new NCommand(this.peer, 2, null, 0xff);
            this.peer.QueueOutgoingReliableCommand(command);
            this.Receive(socketAsyncEventArgs);
        }

        private void ProcessReceive(SocketAsyncEventArgs e)
        {
            if (!this.obsolete)
            {
                if (e.BytesTransferred < 12)
                {
                }
                if (this.peer.NetworkSimulationSettings.IsSimulationEnabled)
                {
                    byte[] bufferReference = new byte[e.BytesTransferred];
                    Buffer.BlockCopy(e.Buffer, 0, bufferReference, 0, e.BytesTransferred);
                    this.peer.ReceiveNetworkSimulated(delegate
                    {
                        this.peer.ReceiveIncomingCommands(bufferReference, e.BytesTransferred);
                    });
                }
                else
                {
                    this.peer.ReceiveIncomingCommands(e.Buffer, e.BytesTransferred);
                }
                this.Receive(e);
            }
        }

        private void Receive(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            lock (this.syncer)
            {
                if (!this.obsolete && !this.sock.ReceiveAsync(socketAsyncEventArgs))
                {
                    this.SocketCompletedEvent(this, socketAsyncEventArgs);
                }
            }
        }

        /// <summary>
        /// used by PhotonPeer*
        /// </summary>
        /// <param name="data"></param>
        /// <param name="length"></param>
        internal void SendUdpPackage(byte[] data, int length)
        {
            lock (this.syncer)
            {
                if (!this.sock.Connected)
                {
                    if (!this.obsolete)
                    {
                        this.peer.EnqueueDebugReturn(DebugLevel.ERROR, "Trying to send but socket is not connected. PeerConnectionState: " + this.peer != null ? this.peer.peerConnectionState.ToString() : "No peer.");
                    }
                }
                else
                {
                    this.sock.Send(data, 0, length, SocketFlags.None);
                }
            }
        }

        private void SocketCompletedEvent(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError != SocketError.Success)
            {
                this.HandleSocketErrrors(e);
            }
            else
            {
                switch (e.LastOperation)
                {
                    case SocketAsyncOperation.Connect:
                        this.ProcessConnect(e);
                        return;

                    case SocketAsyncOperation.Receive:
                        this.ProcessReceive(e);
                        return;

                    case SocketAsyncOperation.Send:
                        return;
                }
                throw new Exception("Invalid operation completed");
            }
        }

        internal bool StartConnection()
        {
            if (this.isRunning)
            {
                if (this.peer.debugOut >= DebugLevel.ERROR)
                {
                    this.peer.Listener.DebugReturn(DebugLevel.ERROR, "StartConnection() failed: connection still open.");
                }
                return false;
            }
            try
            {
                this.serverEndPoint = PeerBase.GetEndpoint(this.peer.ServerAddress);
                if (this.serverEndPoint == null)
                {
                    if (this.peer.debugOut >= DebugLevel.ERROR)
                    {
                        this.peer.Listener.DebugReturn(DebugLevel.ERROR, "StartConnection() failed. Address must be 'address:port'. Is: " + this.peer.ServerAddress);
                    }
                    return false;
                }
                this.obsolete = false;
                this.isRunning = true;
                SocketAsyncEventArgs socketArguments = new SocketAsyncEventArgs();
                socketArguments.Completed += new EventHandler<SocketAsyncEventArgs>(this.SocketCompletedEvent);
                socketArguments.RemoteEndPoint = this.serverEndPoint;
                byte[] udpPacketBuffer = new byte[(this.peer.mtu < 0x240) ? 0x240 : this.peer.mtu];
                socketArguments.SetBuffer(udpPacketBuffer, 0, udpPacketBuffer.Length);
                this.sock.Blocking = false;
                this.sock.Connect(this.serverEndPoint);
                this.ProcessConnect(socketArguments);
            }
            catch (SecurityException se)
            {
                if (this.peer.debugOut >= DebugLevel.ERROR)
                {
                    this.peer.Listener.DebugReturn(DebugLevel.ERROR, "Connect() failed: " + se.ToString());
                }
                this.peer.Listener.OnStatusChanged(StatusCode.SecurityExceptionOnConnect);
                this.peer.Listener.OnStatusChanged(StatusCode.Disconnect);
                return false;
            }
            catch (Exception se)
            {
                if (this.peer.debugOut >= DebugLevel.ERROR)
                {
                    this.peer.Listener.DebugReturn(DebugLevel.ERROR, "Connect() failed: " + se.ToString());
                }
                this.peer.Listener.OnStatusChanged(StatusCode.ExceptionOnConnect);
                this.peer.Listener.OnStatusChanged(StatusCode.Disconnect);
                return false;
            }
            return true;
        }

        internal void StopConnection()
        {
            if (this.peer.debugOut >= DebugLevel.INFO)
            {
                this.peer.Listener.DebugReturn(DebugLevel.INFO, "StopConnection()");
            }
            lock (this.syncer)
            {
                this.obsolete = true;
                this.sock.Close();
            }
            this.peer.Disconnected();
        }
    }
}
