namespace ExitGames.Client.Photon
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    internal class TPeer : PeerBase
    {
        private List<byte[]> incomingList;

        private int lastPingResult;

        internal static readonly byte[] messageHeader = tcpHead;

        internal MemoryStream outgoingStream;

        private byte[] pingRequest;

        internal TConnect rt;

        internal static readonly byte[] tcpHead = new byte[] { 0xfb, 0, 0, 0, 0, 0, 0, 0xf3, 2 };

        internal TPeer()
        {
            this.incomingList = new List<byte[]>();
            this.pingRequest = new byte[5] { 240, 0, 0, 0, 0 };
            PeerBase.peerCount = (short)(PeerBase.peerCount + 1);
            base.InitOnce();
        }

        internal TPeer(IPhotonPeerListener listener)
            : this()
        {
            base.Listener = listener;
        }

        internal override bool Connect(string serverAddress, string appID)
        {
            if (base.peerConnectionState != PeerBase.ConnectionStateValue.Disconnected)
            {
                base.Listener.DebugReturn(DebugLevel.WARNING, "Connect() can't be called if peer is not Disconnected. Not connecting.");
                return false;
            }
            if (base.debugOut >= DebugLevel.ALL)
            {
                base.Listener.DebugReturn(DebugLevel.ALL, "Connect()");
            }
            base.ServerAddress = serverAddress;
            this.InitPeerBase();
            this.outgoingStream = new MemoryStream(PeerBase.outgoingStreamBufferSize);
            if (appID == null)
            {
                appID = "Lite";
            }
            for (int i = 0; i < 0x20; i++)
            {
                base.INIT_BYTES[i + 9] = i < appID.Length ? (byte)appID[i] : (byte)0;
            }
            this.rt = new TConnect(this, base.ServerAddress);
            base.peerConnectionState = PeerBase.ConnectionStateValue.Connecting;
            if (this.rt.StartConnection())
            {
                this.EnqueueInit();
                this.SendOutgoingCommands();
                return true;
            }
            base.peerConnectionState = PeerBase.ConnectionStateValue.Disconnected;
            return false;
        }

        internal override void Disconnect()
        {
            if ((base.peerConnectionState != PeerBase.ConnectionStateValue.Disconnected) && (base.peerConnectionState != PeerBase.ConnectionStateValue.Disconnecting))
            {
                if (base.debugOut >= DebugLevel.ALL)
                {
                    base.Listener.DebugReturn(DebugLevel.ALL, "Disconnect()");
                }
                base.peerConnectionState = PeerBase.ConnectionStateValue.Disconnecting;
                this.rt.StopConnection();
            }
        }

        internal void Disconnected()
        {
            this.InitPeerBase();
            base.Listener.OnStatusChanged(StatusCode.Disconnect);
        }

        //<summary>
        // Checks the incoming queue and Dispatches received data if possible. Returns if a Dispatch happened or
        // not, which shows if more Dispatches might be needed.
        //</summary>
        internal override bool DispatchIncomingCommands()
        {
            while (true)
            {
                PeerBase.MyAction action;
                lock (base.ActionQueue)
                {
                    if (base.ActionQueue.Count <= 0)
                    {
                        byte[] payload;
                        lock (this.incomingList)
                        {
                            if (this.incomingList.Count <= 0)
                            {
                                return false;
                            }
                            payload = this.incomingList[0];
                            this.incomingList.RemoveAt(0);
                        }
                        base.ByteCountCurrentDispatch = payload.Length + 3;
                        return this.DeserializeMessageAndCallback(payload);
                    }
                    action = base.ActionQueue.Dequeue();
                }
                action();
            }
        }

        private void EnqueueInit()
        {
            MemoryStream bout = new MemoryStream(0);
            BinaryWriter bsw = new BinaryWriter(bout);
            byte[] tcpheader = new byte[7] { 0xfb, 0, 0, 0, 0, 0, 0x1 };
            int offsetForLength = 1;
            Protocol.Serialize((int)(base.INIT_BYTES.Length + tcpheader.Length), tcpheader, ref offsetForLength);
            bsw.Write(tcpheader);
            bsw.Write(base.INIT_BYTES);
            byte[] init = bout.ToArray();
            if (base.TrafficStatsEnabled)
            {
                base.TrafficStatsOutgoing.TotalPacketCount++;
                base.TrafficStatsOutgoing.TotalCommandsInPackets++;
                base.TrafficStatsOutgoing.CountControlCommand(init.Length);
            }
            this.EnqueueMessageAsPayload(true, init, 0);
        }

        /// <summary>
        /// enqueues serialized operations to be sent as tcp stream / package
        /// </summary>
        /// <param name="sendReliable"></param>
        /// <param name="opMessage"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        internal bool EnqueueMessageAsPayload(bool sendReliable, byte[] opMessage, byte channelId)
        {
            if (opMessage == null)
            {
                return false;
            }
            opMessage[5] = channelId;
            opMessage[6] = sendReliable ? ((byte)1) : ((byte)0);
            lock (this.outgoingStream)
            {
                this.outgoingStream.Write(opMessage, 0, opMessage.Length);
                base.outgoingCommandsInStream++;
                if ((base.outgoingCommandsInStream % base.warningSize) == 0)
                {
                    base.Listener.OnStatusChanged(StatusCode.QueueOutgoingReliableWarning);
                }
            }
            base.ByteCountLastOperation = opMessage.Length;
            if (base.TrafficStatsEnabled)
            {
                if (sendReliable)
                {
                    base.TrafficStatsOutgoing.CountReliableOpCommand(opMessage.Length);
                }
                else
                {
                    base.TrafficStatsOutgoing.CountUnreliableOpCommand(opMessage.Length);
                }
                base.TrafficStatsGameLevel.CountOperation(opMessage.Length);
            }
            return true;
        }

        internal override bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, bool sendReliable, byte channelId, bool encrypt, PeerBase.EgMessageType messageType)
        {
            if (base.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
            {
                if (base.debugOut >= DebugLevel.ERROR)
                {
                    base.Listener.DebugReturn(DebugLevel.ERROR, string.Concat("Cannot send op: ", opCode, "! Not connected. PeerState: ", base.peerConnectionState));
                }
                base.Listener.OnStatusChanged(StatusCode.SendError);
                return false;
            }
            if (channelId >= base.ChannelCount)
            {
                if (base.debugOut >= DebugLevel.ERROR)
                {
                    base.Listener.DebugReturn(DebugLevel.ERROR, string.Concat("Cannot send op: Selected channel (", channelId, ")>= channelCount (", base.ChannelCount, ")."));
                }
                base.Listener.OnStatusChanged(StatusCode.SendError);
                return false;
            }
            byte[] opBytes = this.SerializeOperationToMessage(opCode, parameters, messageType, encrypt);
            return this.EnqueueMessageAsPayload(sendReliable, opBytes, channelId);
        }

        internal override void FetchServerTimestamp()
        {
            if (base.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
            {
                if (base.debugOut >= DebugLevel.INFO)
                {
                    base.Listener.DebugReturn(DebugLevel.INFO, "FetchServerTimestamp() was skipped, as the client is not connected. Current ConnectionState: " + base.peerConnectionState);
                }
                base.Listener.OnStatusChanged(StatusCode.SendError);
            }
            else
            {
                this.SendPing();
                base.serverTimeOffsetIsAvailable = false;
            }
        }

        internal override void InitPeerBase()
        {
            base.InitPeerBase();
            this.incomingList = new List<byte[]>();
        }

        private void ReadPingResult(byte[] inbuff)
        {
            int serverSentTime = 0;
            int clientSentTime = 0;
            int offset = 1;
            Protocol.Deserialize(out serverSentTime, inbuff, ref offset);
            Protocol.Deserialize(out clientSentTime, inbuff, ref offset);
            base.lastRoundTripTime = SupportClass.GetTickCount() - clientSentTime;
            if (!base.serverTimeOffsetIsAvailable)
            {
                base.roundTripTime = base.lastRoundTripTime;
            }
            base.UpdateRoundTripTimeAndVariance(base.lastRoundTripTime);
            if (!base.serverTimeOffsetIsAvailable)
            {
                base.serverTimeOffset = (serverSentTime + (base.lastRoundTripTime >> 1)) - SupportClass.GetTickCount();
                base.serverTimeOffsetIsAvailable = true;
            }
        }

        /// <summary>
        /// reads incoming tcp-packages to create and queue incoming commands*
        /// </summary>
        /// <param name="inbuff"></param>
        /// <param name="dataLength"></param>
        internal override void ReceiveIncomingCommands(byte[] inbuff, int dataLength)
        {
            if (inbuff == null)
            {
                if (base.debugOut >= DebugLevel.ERROR)
                {
                    base.EnqueueDebugReturn(DebugLevel.ERROR, "checkAndQueueIncomingCommands() inBuff: null");
                }
            }
            else
            {
                base.timestampOfLastReceive = SupportClass.GetTickCount();
                base.bytesIn += inbuff.Length + 7;
                if (base.TrafficStatsEnabled)
                {
                    base.TrafficStatsIncoming.TotalPacketCount++;
                    base.TrafficStatsIncoming.TotalCommandsInPackets++;
                }
                if ((inbuff[0] == 0xf3) || (inbuff[0] == 0xf4))
                {
                    lock (this.incomingList)
                    {
                        this.incomingList.Add(inbuff);
                        if ((this.incomingList.Count % base.warningSize) == 0)
                        {
                            base.EnqueueStatusCallback(StatusCode.QueueIncomingReliableWarning);
                        }
                    }
                }
                else if (inbuff[0] == 240)
                {
                    this.ReadPingResult(inbuff);
                }
                else if (base.debugOut >= DebugLevel.ERROR)
                {
                    base.EnqueueDebugReturn(DebugLevel.ERROR, "receiveIncomingCommands() MagicNumber should be 0xF0, 0xF3 or 0xF4. Is: " + inbuff[0]);
                }
            }
        }

        internal void SendData(byte[] data)
        {
            try
            {
                base.bytesOut += data.Length;
                if (base.TrafficStatsEnabled)
                {
                    base.TrafficStatsOutgoing.TotalPacketCount++;
                    base.TrafficStatsOutgoing.TotalCommandsInPackets += base.outgoingCommandsInStream;
                }
                if (base.NetworkSimulationSettings.IsSimulationEnabled)
                {
                    base.SendNetworkSimulated(delegate
                    {
                        this.rt.sendTcp(data);
                    });
                }
                else
                {
                    this.rt.sendTcp(data);
                }
            }
            catch (Exception ex)
            {
                if (base.debugOut >= DebugLevel.ERROR)
                {
                    base.Listener.DebugReturn(DebugLevel.ERROR, ex.ToString());
                }
                SupportClass.WriteStackTrace(ex, null);
            }
        }

        /// <summary>
        /// gathers commands from all (out)queues until udp-packet is full and sends it!
        /// </summary>
        /// <returns></returns>
        internal override bool SendOutgoingCommands()
        {
            if (base.peerConnectionState != PeerBase.ConnectionStateValue.Disconnected)
            {
                if (!this.rt.isRunning)
                {
                    return false;
                }
                if ((base.peerConnectionState == PeerBase.ConnectionStateValue.Connected) && ((SupportClass.GetTickCount() - this.lastPingResult) > base.timePingInterval))
                {
                    this.SendPing();
                }
                lock (this.outgoingStream)
                {
                    if (this.outgoingStream.Position > 0L)
                    {
                        this.SendData(this.outgoingStream.ToArray());
                        this.outgoingStream.Position = 0L;
                        this.outgoingStream.SetLength(0L);
                        base.outgoingCommandsInStream = 0;
                    }
                }
            }
            return false;
        }

        internal void SendPing()
        {
            int offset = 1;
            Protocol.Serialize(SupportClass.GetTickCount(), this.pingRequest, ref offset);
            this.lastPingResult = SupportClass.GetTickCount();
            if (base.TrafficStatsEnabled)
            {
                base.TrafficStatsOutgoing.CountControlCommand(this.pingRequest.Length);
            }
            this.SendData(this.pingRequest);
        }

        /// <summary>
        /// Returns the UDP Payload starting with Magic Number for binary protocol 
        /// </summary>
        /// <param name="opc"></param>
        /// <param name="parameters"></param>
        /// <param name="messageType"></param>
        /// <param name="encrypt"></param>
        /// <returns></returns>
        internal override byte[] SerializeOperationToMessage(byte opc, Dictionary<byte, object> parameters, PeerBase.EgMessageType messageType, bool encrypt)
        {
            byte[] fullMessageBytes;
            lock (base.SerializeMemStream)
            {
                base.SerializeMemStream.Position = 0L;
                base.SerializeMemStream.SetLength(0L);
                if (!encrypt)
                {
                    base.SerializeMemStream.Write(messageHeader, 0, messageHeader.Length);
                }
                Protocol.SerializeOperationRequest(base.SerializeMemStream, opc, parameters, false);
                if (encrypt)
                {
                    byte[] opBytes = base.SerializeMemStream.ToArray();
                    opBytes = base.CryptoProvider.Encrypt(opBytes);
                    base.SerializeMemStream.Position = 0L;
                    base.SerializeMemStream.SetLength(0L);
                    base.SerializeMemStream.Write(messageHeader, 0, messageHeader.Length);
                    base.SerializeMemStream.Write(opBytes, 0, opBytes.Length);
                }
                fullMessageBytes = base.SerializeMemStream.ToArray();
            }
            if (messageType != PeerBase.EgMessageType.Operation)
            {
                fullMessageBytes[messageHeader.Length - 1] = (byte)messageType;
            }
            if (encrypt)
            {
                fullMessageBytes[messageHeader.Length - 1] = (byte)(fullMessageBytes[messageHeader.Length - 1] | 0x80);
            }
            int offsetForLength = 1;
            Protocol.Serialize(fullMessageBytes.Length, fullMessageBytes, ref offsetForLength);
            return fullMessageBytes;
        }

        internal override void StopConnection()
        {
            this.rt.StopConnection();
        }

        internal override int QueuedIncomingCommandsCount
        {
            get
            {
                return this.incomingList.Count;
            }
        }

        internal override int QueuedOutgoingCommandsCount
        {
            get
            {
                return base.outgoingCommandsInStream;
            }
        }
    }
}
