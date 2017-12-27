namespace ExitGames.Client.Photon
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class EnetPeer : PeerBase
    {
        /// <summary>
        /// Will contain channel 0xFF and any other.
        /// </summary>
        private Dictionary<byte, EnetChannel> channels;

        /// <summary>
        /// One list for all channels keeps sent commands (for re-sending).
        /// </summary>
        private List<NCommand> sentReliableCommands;

        /// <summary>
        /// One list for all channels keeps acknowledgements.
        /// </summary>
        private Queue<NCommand> outgoingAcknowledgementsList;

        internal int challenge;
        private const int CRC_LENGTH = 4;
        internal static readonly byte[] messageHeader = udpHeader0xF3;
        internal int reliableCommandsRepeated;
        internal int reliableCommandsSent;
        internal NConnect rt;
        internal int serverSentTime;
        private byte[] udpBuffer;
        private int udpBufferIndex;
        private byte udpCommandCount;
        internal static readonly byte[] udpHeader0xF3 = new byte[] { 0xf3, 2 };
        internal readonly int windowSize;

        internal EnetPeer()
        {
            this.channels = new Dictionary<byte, EnetChannel>();
            this.sentReliableCommands = new List<NCommand>();
            this.outgoingAcknowledgementsList = new Queue<NCommand>();
            this.windowSize = 0x80;
            PeerBase.peerCount = (short)(PeerBase.peerCount + 1);
            base.InitOnce();
            base.TrafficPackageHeaderSize = 12;
        }

        internal EnetPeer(IPhotonPeerListener listener)
            : this()
        {
            base.Listener = listener;
        }

        //<summary>
        // Checks if any channel has a outgoing reliable command.
        //</summary>
        //<returns>True if any channel has a outgoing reliable command. False otherwise.</returns>
        private bool AreReliableCommandsInTransit()
        {
            lock (this.channels)
            {
                foreach (EnetChannel channel in this.channels.Values)
                {
                    if (channel.outgoingReliableCommandsList.Count > 0)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal string CommandListToString(NCommand[] list)
        {
            if (base.debugOut < DebugLevel.ALL)
            {
                return string.Empty;
            }
            StringBuilder tmp = new StringBuilder();
            for (int i = 0; i < list.Length; i++)
            {
                tmp.Append(i + "=");
                tmp.Append(list[i]);
                tmp.Append(" # ");
            }
            return tmp.ToString();
        }

        internal override bool Connect(string ipport, string appID)
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
            base.ServerAddress = ipport;
            this.InitPeerBase();
            if (appID == null)
            {
                appID = "Lite";
            }
            for (int i = 0; i < 0x20; i++)
            {
                base.INIT_BYTES[i + 9] = (i < appID.Length) ? ((byte)appID[i]) : ((byte)0);
            }
            this.rt = new NConnect(this);
            if (this.rt.StartConnection())
            {
                if (base.TrafficStatsEnabled)
                {
                    base.TrafficStatsOutgoing.ControlCommandBytes += 0x2c;
                    base.TrafficStatsOutgoing.ControlCommandCount++;
                }
                base.peerConnectionState = PeerBase.ConnectionStateValue.Connecting;
                return true;
            }
            return false;
        }

        //<summary>reliable-udp-level function to send some byte[] to the server via un/reliable command</summary>
        //<remarks>only called when a custom operation should be send</remarks>
        //<param name="commandType">(enet) command type</param>
        //<param name="payload">data to carry (operation)</param>
        //<param name="channelNumber">channel in which to send</param>
        //<returns>the invocation ID for this operation (the payload)</returns>
        internal bool CreateAndEnqueueCommand(byte commandType, byte[] payload, byte channelNumber)
        {
            NCommand command;
            if (payload == null)
            {
                return false;
            }
            EnetChannel channel = this.channels[channelNumber];
            base.ByteCountLastOperation = 0;
            int fragmentLength = (base.mtu - 12) - 0x20;
            if (payload.Length > fragmentLength)
            {
                int fragmentCount = ((payload.Length + fragmentLength) - 1) / fragmentLength;
                int startSequenceNumber = channel.outgoingReliableSequenceNumber + 1;
                int fragmentNumber = 0;
                for (int fragmentOffset = 0; fragmentOffset < payload.Length; fragmentOffset += fragmentLength)
                {
                    if ((payload.Length - fragmentOffset) < fragmentLength)
                    {
                        fragmentLength = payload.Length - fragmentOffset;
                    }
                    byte[] tmpPayload = new byte[fragmentLength];
                    Buffer.BlockCopy(payload, fragmentOffset, tmpPayload, 0, fragmentLength);
                    command = new NCommand(this, 8, tmpPayload, channel.ChannelNumber);
                    command.fragmentNumber = fragmentNumber;
                    command.startSequenceNumber = startSequenceNumber;
                    command.fragmentCount = fragmentCount;
                    command.totalLength = payload.Length;
                    command.fragmentOffset = fragmentOffset;
                    this.QueueOutgoingReliableCommand(command);
                    base.ByteCountLastOperation += command.Size;
                    if (base.TrafficStatsEnabled)
                    {
                        base.TrafficStatsOutgoing.CountFragmentOpCommand(command.Size);
                        base.TrafficStatsGameLevel.CountOperation(command.Size);
                    }
                    fragmentNumber++;
                }
            }
            else
            {
                command = new NCommand(this, commandType, payload, channel.ChannelNumber);
                if (command.commandFlags == 1)
                {
                    this.QueueOutgoingReliableCommand(command);
                    base.ByteCountLastOperation = command.Size;
                    if (base.TrafficStatsEnabled)
                    {
                        base.TrafficStatsOutgoing.CountReliableOpCommand(command.Size);
                        base.TrafficStatsGameLevel.CountOperation(command.Size);
                    }
                }
                else
                {
                    this.QueueOutgoingUnreliableCommand(command);
                    base.ByteCountLastOperation = command.Size;
                    if (base.TrafficStatsEnabled)
                    {
                        base.TrafficStatsOutgoing.CountUnreliableOpCommand(command.Size);
                        base.TrafficStatsGameLevel.CountOperation(command.Size);
                    }
                }
            }
            return true;
        }

        internal override void Disconnect()
        {
            if ((base.peerConnectionState != PeerBase.ConnectionStateValue.Disconnected) && (base.peerConnectionState != PeerBase.ConnectionStateValue.Disconnecting))
            {
                if (base.debugOut >= DebugLevel.INFO)
                {
                    base.Listener.DebugReturn(DebugLevel.INFO, "Disconnect()");
                }
                if (this.outgoingAcknowledgementsList != null)
                {
                    lock (this.outgoingAcknowledgementsList)
                    {
                        this.outgoingAcknowledgementsList.Clear();
                    }
                }
                if (this.sentReliableCommands != null)
                {
                    lock (this.sentReliableCommands)
                    {
                        this.sentReliableCommands.Clear();
                    }
                }
                lock (this.channels)
                {
                    foreach (EnetChannel c in this.channels.Values)
                    {
                        c.clearAll();
                    }
                }
                NCommand command = new NCommand(this, 4, null, 0xff);
                this.QueueOutgoingReliableCommand(command);
                if (base.TrafficStatsEnabled)
                {
                    base.TrafficStatsOutgoing.CountControlCommand(command.Size);
                }
                this.SendOutgoingCommands();
                if (base.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
                {
                    this.rt.StopConnection();
                }
                else
                {
                    base.peerConnectionState = PeerBase.ConnectionStateValue.Disconnecting;
                }
            }
        }

        internal void Disconnected()
        {
            this.InitPeerBase();
            base.Listener.OnStatusChanged(StatusCode.Disconnect);
        }

        //<summary>
        // Checks the incoming queue and Dispatches received data if possible.
        //</summary>
        //<returns>If a Dispatch happened or not, which shows if more Dispatches might be needed.</returns>
        internal override bool DispatchIncomingCommands()
        {
            NCommand command;
            while (true)
            {
                PeerBase.MyAction action;
                lock (base.ActionQueue)
                {
                    if (base.ActionQueue.Count <= 0)
                    {
                        command = null;
                        Queue<int> commandsToRemove = new Queue<int>();
                        lock (this.channels)
                        {
                            foreach (EnetChannel channel in this.channels.Values)
                            {
                                if (channel.incomingUnreliableCommandsList.Count > 0)
                                {
                                    int lowestAvailableUnreliableCommandNumber = 0x7fffffff;
                                    foreach (int sequenceNumber in channel.incomingUnreliableCommandsList.Keys)
                                    {
                                        NCommand cmd = channel.incomingUnreliableCommandsList[sequenceNumber];
                                        if ((sequenceNumber < channel.incomingUnreliableSequenceNumber) || (cmd.reliableSequenceNumber < channel.incomingReliableSequenceNumber))
                                        {
                                            commandsToRemove.Enqueue(sequenceNumber);
                                            continue;
                                        }
                                        if ((base.limitOfUnreliableCommands > 0) && (channel.incomingUnreliableCommandsList.Count > base.limitOfUnreliableCommands))
                                        {
                                            commandsToRemove.Enqueue(sequenceNumber);
                                            continue;
                                        }
                                        if (sequenceNumber < lowestAvailableUnreliableCommandNumber)
                                        {
                                            if (cmd.reliableSequenceNumber > channel.incomingReliableSequenceNumber)
                                            {
                                                continue;
                                            }
                                            lowestAvailableUnreliableCommandNumber = sequenceNumber;
                                        }
                                    }
                                    while (commandsToRemove.Count > 0)
                                    {
                                        channel.incomingUnreliableCommandsList.Remove(commandsToRemove.Dequeue());
                                    }
                                    if (lowestAvailableUnreliableCommandNumber < 0x7fffffff)
                                    {
                                        command = channel.incomingUnreliableCommandsList[lowestAvailableUnreliableCommandNumber];
                                    }
                                    if (command != null)
                                    {
                                        channel.incomingUnreliableCommandsList.Remove(command.unreliableSequenceNumber);
                                        channel.incomingUnreliableSequenceNumber = command.unreliableSequenceNumber;
                                        goto Label_03DA;
                                    }
                                }
                                if ((command == null) && (channel.incomingReliableCommandsList.Count > 0))
                                {
                                    channel.incomingReliableCommandsList.TryGetValue(channel.incomingReliableSequenceNumber + 1, out command);
                                    if (command == null)
                                    {
                                        continue;
                                    }
                                    if (command.commandType != 8)
                                    {
                                        channel.incomingReliableSequenceNumber = command.reliableSequenceNumber;
                                        channel.incomingReliableCommandsList.Remove(command.reliableSequenceNumber);
                                    }
                                    else if (command.fragmentsRemaining > 0)
                                    {
                                        command = null;
                                    }
                                    else
                                    {
                                        byte[] completePayload = new byte[command.totalLength];
                                        for (int fragmentSequenceNumber = command.startSequenceNumber; fragmentSequenceNumber < (command.startSequenceNumber + command.fragmentCount); fragmentSequenceNumber++)
                                        {
                                            if (!channel.ContainsReliableSequenceNumber(fragmentSequenceNumber))
                                            {
                                                throw new Exception("command.fragmentsRemaining was 0, but not all fragments are found to be combined!");
                                            }
                                            NCommand fragment = channel.FetchReliableSequenceNumber(fragmentSequenceNumber);
                                            Buffer.BlockCopy(fragment.Payload, 0, completePayload, fragment.fragmentOffset, fragment.Payload.Length);
                                            channel.incomingReliableCommandsList.Remove(fragment.reliableSequenceNumber);
                                        }
                                        if (base.debugOut >= DebugLevel.ALL)
                                        {
                                            base.Listener.DebugReturn(DebugLevel.ALL, "assembled fragmented payload from " + command.fragmentCount + " parts. Dispatching now.");
                                        }
                                        command.Payload = completePayload;
                                        command.Size = (12 * command.fragmentCount) + command.totalLength;
                                        channel.incomingReliableSequenceNumber = (command.reliableSequenceNumber + command.fragmentCount) - 1;
                                    }
                                    goto Label_03DA;
                                }
                            }
                        }
                        break;
                    }
                    action = base.ActionQueue.Dequeue();
                }
                action();
            }
        Label_03DA:
            if ((command != null) && (command.Payload != null))
            {
                base.ByteCountCurrentDispatch = command.Size;
                if (this.DeserializeMessageAndCallback(command.Payload))
                {
                    return true;
                }
            }
            return false;
        }

        //<summary>
        // Checks connected state and channel before operation is serialized and enqueued for sending.
        //</summary>
        //<param name="parameters">operation parameters</param>
        //<param name="opCode">code of operation</param>
        //<param name="sendReliable">send as reliable command</param>
        //<param name="channelId">channel (sequence) for command</param>
        //<param name="encrypt">encrypt or not</param>
        //<param name="messageType">usually EgMessageType.Operation</param>
        //<returns>if operation could be enqueued</returns>
        internal override bool EnqueueOperation(Dictionary<byte, object> parameters, byte opCode, bool sendReliable, byte channelId, bool encrypt, PeerBase.EgMessageType messageType)
        {
            if (base.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
            {
                if (base.debugOut >= DebugLevel.ERROR)
                {
                    base.Listener.DebugReturn(DebugLevel.ERROR, string.Concat(new object[] { "Cannot send op: ", opCode, " Not connected. PeerState: ", base.peerConnectionState }));
                }
                base.Listener.OnStatusChanged(StatusCode.SendError);
                return false;
            }
            if (channelId >= base.ChannelCount)
            {
                if (base.debugOut >= DebugLevel.ERROR)
                {
                    base.Listener.DebugReturn(DebugLevel.ERROR, string.Concat(new object[] { "Cannot send op: Selected channel (", channelId, ")>= channelCount (", base.ChannelCount, ")." }));
                }
                base.Listener.OnStatusChanged(StatusCode.SendError);
                return false;
            }
            byte[] opBytes = this.SerializeOperationToMessage(opCode, parameters, messageType, encrypt);
            return this.CreateAndEnqueueCommand(sendReliable ? ((byte)6) : ((byte)7), opBytes, channelId);
        }

        internal bool ExecuteCommand(NCommand command)
        {
            bool success = true;
            switch (command.commandType)
            {
                case 1:
                    {
                        if (base.TrafficStatsEnabled)
                        {
                            base.TrafficStatsIncoming.CountControlCommand(command.Size);
                        }
                        base.timeLastAckReceive = base.timeInt;
                        base.lastRoundTripTime = base.timeInt - command.ackReceivedSentTime;
                        NCommand removedCommand = this.RemoveSentReliableCommand(command.ackReceivedReliableSequenceNumber, command.commandChannelID);
                        if (removedCommand != null)
                        {
                            if (removedCommand.commandType != 12)
                            {
                                base.UpdateRoundTripTimeAndVariance(base.lastRoundTripTime);
                                if ((removedCommand.commandType == 4) && (base.peerConnectionState == PeerBase.ConnectionStateValue.Disconnecting))
                                {
                                    if (base.debugOut >= DebugLevel.INFO)
                                    {
                                        base.EnqueueDebugReturn(DebugLevel.INFO, "Received disconnect ACK by server");
                                    }
                                    base.EnqueueActionForDispatch(delegate
                                    {
                                        this.rt.StopConnection();
                                    });
                                    return success;
                                }
                                if (removedCommand.commandType == 2)
                                {
                                    base.roundTripTime = base.lastRoundTripTime;
                                }
                                return success;
                            }
                            if (base.lastRoundTripTime <= base.roundTripTime)
                            {
                                base.serverTimeOffset = (this.serverSentTime + (base.lastRoundTripTime >> 1)) - SupportClass.GetTickCount();
                                base.serverTimeOffsetIsAvailable = true;
                                return success;
                            }
                            this.FetchServerTimestamp();
                        }
                        return success;
                    }
                case 2:
                case 5:
                    if (base.TrafficStatsEnabled)
                    {
                        base.TrafficStatsIncoming.CountControlCommand(command.Size);
                    }
                    return success;

                case 3:
                    if (base.TrafficStatsEnabled)
                    {
                        base.TrafficStatsIncoming.CountControlCommand(command.Size);
                    }
                    if (base.peerConnectionState == PeerBase.ConnectionStateValue.Connecting)
                    {
                        command = new NCommand(this, 6, base.INIT_BYTES, 0);
                        this.QueueOutgoingReliableCommand(command);
                        if (base.TrafficStatsEnabled)
                        {
                            base.TrafficStatsOutgoing.CountControlCommand(command.Size);
                        }
                        base.peerConnectionState = PeerBase.ConnectionStateValue.Connected;
                    }
                    return success;

                case 4:
                    {
                        if (base.TrafficStatsEnabled)
                        {
                            base.TrafficStatsIncoming.CountControlCommand(command.Size);
                        }
                        StatusCode reason = StatusCode.DisconnectByServer;
                        if (command.reservedByte == 1)
                        {
                            reason = StatusCode.DisconnectByServerLogic;
                        }
                        else if (command.reservedByte == 3)
                        {
                            reason = StatusCode.DisconnectByServerUserLimit;
                        }
                        if (base.debugOut >= DebugLevel.INFO)
                        {
                            base.Listener.DebugReturn(DebugLevel.INFO, string.Concat(new object[] { "Server sent disconnect. PeerId: ", base.peerID, " RTT/Variance:", base.roundTripTime, "/", base.roundTripTimeVariance }));
                        }
                        base.peerConnectionState = PeerBase.ConnectionStateValue.Disconnecting;
                        base.Listener.OnStatusChanged(reason);
                        this.rt.StopConnection();
                        return success;
                    }
                case 6:
                    if (base.TrafficStatsEnabled)
                    {
                        base.TrafficStatsIncoming.CountReliableOpCommand(command.Size);
                    }
                    if (base.peerConnectionState == PeerBase.ConnectionStateValue.Connected)
                    {
                        success = this.QueueIncomingCommand(command);
                    }
                    return success;

                case 7:
                    if (base.TrafficStatsEnabled)
                    {
                        base.TrafficStatsIncoming.CountUnreliableOpCommand(command.Size);
                    }
                    if (base.peerConnectionState == PeerBase.ConnectionStateValue.Connected)
                    {
                        success = this.QueueIncomingCommand(command);
                    }
                    return success;

                case 8:
                    if (base.TrafficStatsEnabled)
                    {
                        base.TrafficStatsIncoming.CountFragmentOpCommand(command.Size);
                    }
                    if (base.peerConnectionState == PeerBase.ConnectionStateValue.Connected)
                    {
                        if (((command.fragmentNumber > command.fragmentCount) || (command.fragmentOffset >= command.totalLength)) || ((command.fragmentOffset + command.Payload.Length) > command.totalLength))
                        {
                            if (base.debugOut >= DebugLevel.ERROR)
                            {
                                base.Listener.DebugReturn(DebugLevel.ERROR, "Received fragment has bad size: " + command);
                            }
                            return success;
                        }
                        success = this.QueueIncomingCommand(command);
                        if (!success)
                        {
                            return success;
                        }
                        EnetChannel channel = this.channels[command.commandChannelID];
                        if (command.reliableSequenceNumber != command.startSequenceNumber)
                        {
                            if (channel.ContainsReliableSequenceNumber(command.startSequenceNumber))
                            {
                                NCommand command1 = channel.FetchReliableSequenceNumber(command.startSequenceNumber);
                                command1.fragmentsRemaining--;
                            }
                            return success;
                        }
                        command.fragmentsRemaining--;
                        int fragmentSequenceNumber = command.startSequenceNumber + 1;
                        while ((command.fragmentsRemaining > 0) && (fragmentSequenceNumber < (command.startSequenceNumber + command.fragmentCount)))
                        {
                            if (channel.ContainsReliableSequenceNumber(fragmentSequenceNumber++))
                            {
                                command.fragmentsRemaining--;
                            }
                        }
                    }
                    return success;
            }
            return success;
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
                this.CreateAndEnqueueCommand(12, new byte[0], 0xff);
            }
        }

        internal override void InitPeerBase()
        {
            base.InitPeerBase();
            base.peerID = -1;
            this.challenge = SupportClass.ThreadSafeRandom.Next();
            this.udpBuffer = new byte[base.mtu];
            this.reliableCommandsSent = 0;
            this.reliableCommandsRepeated = 0;
            lock (this.channels)
            {
                this.channels = new Dictionary<byte, EnetChannel>();
            }
            lock (this.channels)
            {
                this.channels[0xff] = new EnetChannel(0xff, base.commandBufferSize);
                for (byte i = 0; i < base.ChannelCount; i = (byte)(i + 1))
                {
                    this.channels[i] = new EnetChannel(i, base.commandBufferSize);
                }
            }
            lock (this.sentReliableCommands)
            {
                this.sentReliableCommands = new List<NCommand>(base.commandBufferSize);
            }
            lock (this.outgoingAcknowledgementsList)
            {
                this.outgoingAcknowledgementsList = new Queue<NCommand>(base.commandBufferSize);
            }
        }

        /// <summary>
        /// queues incoming commands in the correct order as either unreliable, reliable or unsequenced. return value determines if the command is queued / done.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        internal bool QueueIncomingCommand(NCommand command)
        {
            EnetChannel channel = null;
            this.channels.TryGetValue(command.commandChannelID, out channel);
            if (channel == null)
            {
                if (base.debugOut >= DebugLevel.ERROR)
                {
                    base.Listener.DebugReturn(DebugLevel.ERROR, "Received command for non-existing channel: " + command.commandChannelID);
                }
                return false;
            }
            if (base.debugOut >= DebugLevel.ALL)
            {
                base.Listener.DebugReturn(DebugLevel.ALL, string.Concat(new object[] { "queueIncomingCommand( ", command, " )  -  incomingReliableSequenceNumber: ", channel.incomingReliableSequenceNumber }));
            }
            if (command.commandFlags == 1)
            {
                if (command.reliableSequenceNumber <= channel.incomingReliableSequenceNumber)
                {
                    if (base.debugOut >= DebugLevel.INFO)
                    {
                        base.Listener.DebugReturn(DebugLevel.INFO, string.Concat(new object[] { "incoming command ", command.ToString(), " is old (not saving it). Dispatched incomingReliableSequenceNumber: ", channel.incomingReliableSequenceNumber }));
                    }
                    return false;
                }
                if (channel.ContainsReliableSequenceNumber(command.reliableSequenceNumber))
                {
                    if (base.debugOut >= DebugLevel.INFO)
                    {
                        base.Listener.DebugReturn(DebugLevel.INFO, string.Concat(new object[] { "Info: command was received before! Old/New: ", channel.FetchReliableSequenceNumber(command.reliableSequenceNumber), "/", command, " inReliableSeq#: ", channel.incomingReliableSequenceNumber }));
                    }
                    return false;
                }
                if ((channel.incomingReliableCommandsList.Count >= base.warningSize) && ((channel.incomingReliableCommandsList.Count % base.warningSize) == 0))
                {
                    base.Listener.OnStatusChanged(StatusCode.QueueIncomingReliableWarning);
                }
                channel.incomingReliableCommandsList.Add(command.reliableSequenceNumber, command);
                return true;
            }
            if (command.commandFlags == 0)
            {
                if (base.debugOut >= DebugLevel.ALL)
                {
                    base.Listener.DebugReturn(DebugLevel.ALL, string.Concat(new object[] { "unreliable. local: ", channel.incomingReliableSequenceNumber, "/", channel.incomingUnreliableSequenceNumber, " incoming: ", command.reliableSequenceNumber, "/", command.unreliableSequenceNumber }));
                }
                if (command.reliableSequenceNumber < channel.incomingReliableSequenceNumber)
                {
                    if (base.debugOut >= DebugLevel.INFO)
                    {
                        base.Listener.DebugReturn(DebugLevel.INFO, "incoming reliable-seq# < Dispatched-rel-seq#. not saved.");
                    }
                    return true;
                }
                if (command.unreliableSequenceNumber <= channel.incomingUnreliableSequenceNumber)
                {
                    if (base.debugOut >= DebugLevel.INFO)
                    {
                        base.Listener.DebugReturn(DebugLevel.INFO, "incoming unreliable-seq# < Dispatched-unrel-seq#. not saved.");
                    }
                    return true;
                }
                if (channel.ContainsUnreliableSequenceNumber(command.unreliableSequenceNumber))
                {
                    if (base.debugOut >= DebugLevel.INFO)
                    {
                        base.Listener.DebugReturn(DebugLevel.INFO, string.Concat(new object[] { "command was received before! Old/New: ", channel.incomingUnreliableCommandsList[command.unreliableSequenceNumber], "/", command }));
                    }
                    return false;
                }
                if ((channel.incomingUnreliableCommandsList.Count >= base.warningSize) && ((channel.incomingUnreliableCommandsList.Count % base.warningSize) == 0))
                {
                    base.Listener.OnStatusChanged(StatusCode.QueueIncomingUnreliableWarning);
                }
                channel.incomingUnreliableCommandsList.Add(command.unreliableSequenceNumber, command);
                return true;
            }
            return false;
        }

        internal void QueueOutgoingAcknowledgement(NCommand command)
        {
            lock (this.outgoingAcknowledgementsList)
            {
                if ((this.outgoingAcknowledgementsList.Count >= base.warningSize) && ((this.outgoingAcknowledgementsList.Count % base.warningSize) == 0))
                {
                    base.Listener.OnStatusChanged(StatusCode.QueueOutgoingAcksWarning);
                }
                this.outgoingAcknowledgementsList.Enqueue(command);
            }
        }

        internal void QueueOutgoingReliableCommand(NCommand command)
        {
            EnetChannel channel = this.channels[command.commandChannelID];
            lock (channel)
            {
                Queue<NCommand> outgoingReliableCommands = channel.outgoingReliableCommandsList;
                if ((outgoingReliableCommands.Count >= base.warningSize) && ((outgoingReliableCommands.Count % base.warningSize) == 0))
                {
                    base.Listener.OnStatusChanged(StatusCode.QueueOutgoingReliableWarning);
                }
                if (command.reliableSequenceNumber == 0)
                {
                    command.reliableSequenceNumber = ++channel.outgoingReliableSequenceNumber;
                }
                outgoingReliableCommands.Enqueue(command);
            }
        }

        internal void QueueOutgoingUnreliableCommand(NCommand command)
        {
            Queue<NCommand> outgoingUnreliableCommands = this.channels[command.commandChannelID].outgoingUnreliableCommandsList;
            if ((outgoingUnreliableCommands.Count >= base.warningSize) && ((outgoingUnreliableCommands.Count % base.warningSize) == 0))
            {
                base.Listener.OnStatusChanged(StatusCode.QueueOutgoingUnreliableWarning);
            }
            EnetChannel channel = this.channels[command.commandChannelID];
            command.reliableSequenceNumber = channel.outgoingReliableSequenceNumber;
            command.unreliableSequenceNumber = ++channel.outgoingUnreliableSequenceNumber;
            outgoingUnreliableCommands.Enqueue(command);
        }

        internal void QueueSentCommand(NCommand command)
        {
            command.commandSentTime = base.timeInt;
            command.commandSentCount = (byte)(command.commandSentCount + 1);
            if (command.roundTripTimeout == 0)
            {
                command.roundTripTimeout = base.roundTripTime + (4 * base.roundTripTimeVariance);
                command.timeoutTime = base.timeInt + base.DisconnectTimeout;
            }
            else
            {
                command.roundTripTimeout *= 2;
            }
            lock (this.sentReliableCommands)
            {
                if (this.sentReliableCommands.Count == 0)
                {
                    base.timeoutInt = command.commandSentTime + command.roundTripTimeout;
                }
                this.reliableCommandsSent++;
                this.sentReliableCommands.Add(command);
            }
            if ((this.sentReliableCommands.Count >= base.warningSize) && ((this.sentReliableCommands.Count % base.warningSize) == 0))
            {
                base.Listener.OnStatusChanged(StatusCode.QueueSentWarning);
            }
        }

        /// <summary>
        /// reads incoming udp-packages to create and queue incoming commands*
        /// </summary>
        /// <param name="inBuff"></param>
        /// <param name="dataLength"></param>
        internal override void ReceiveIncomingCommands(byte[] inBuff, int dataLength)
        {
            base.timestampOfLastReceive = SupportClass.GetTickCount();
            try
            {
                short peerID;
                int inChallenge;
                int readingOffset = 0;
                Protocol.Deserialize(out peerID, inBuff, ref readingOffset);
                byte flags = inBuff[readingOffset++];
                byte commandCount = inBuff[readingOffset++];
                Protocol.Deserialize(out this.serverSentTime, inBuff, ref readingOffset);
                Protocol.Deserialize(out inChallenge, inBuff, ref readingOffset);
                if (flags == 0xcc)
                {
                    int crc;
                    Protocol.Deserialize(out crc, inBuff, ref readingOffset);
                    base.bytesIn += 4L;
                    readingOffset -= 4;
                    Protocol.Serialize(0, inBuff, ref readingOffset);
                    uint localCrc = SupportClass.CalculateCrc(inBuff, dataLength);
                    if (crc != localCrc)
                    {
                        base.packetLossByCrc++;
                        return;
                    }
                }
                base.bytesIn += 12L;
                if (base.TrafficStatsEnabled)
                {
                    base.TrafficStatsIncoming.TotalPacketCount++;
                    base.TrafficStatsIncoming.TotalCommandsInPackets += commandCount;
                }
                if (commandCount > base.commandBufferSize)
                {
                    base.EnqueueDebugReturn(DebugLevel.ALL, string.Concat(new object[] { "too many incoming commands in packet: ", commandCount, " > ", base.commandBufferSize }));
                }
                if (inChallenge != this.challenge)
                {
                    if ((base.peerConnectionState != PeerBase.ConnectionStateValue.Disconnected) && (base.debugOut >= DebugLevel.ALL))
                    {
                        base.EnqueueDebugReturn(DebugLevel.ALL, string.Concat(new object[] { "Info: received package with wrong challenge. challenge in/out:", inChallenge, "!=", this.challenge, " Commands in it: ", commandCount }));
                    }
                }
                else
                {
                    base.timeInt = SupportClass.GetTickCount() - base.timeBase;
                    for (int i = 0; i < commandCount; i++)
                    {
                        NCommand readCommand = new NCommand(this, inBuff, ref readingOffset);
                        if (readCommand.commandType != 1)
                        {
                            base.EnqueueActionForDispatch(delegate
                            {
                                this.ExecuteCommand(readCommand);
                            });
                        }
                        else
                        {
                            this.ExecuteCommand(readCommand);
                        }
                        if ((readCommand.commandFlags & 1) > 0)
                        {
                            NCommand ackForCommand = NCommand.CreateAck(this, readCommand, this.serverSentTime);
                            this.QueueOutgoingAcknowledgement(ackForCommand);
                            if (base.TrafficStatsEnabled)
                            {
                                base.TrafficStatsOutgoing.CountControlCommand(ackForCommand.Size);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (base.debugOut >= DebugLevel.ERROR)
                {
                    base.EnqueueDebugReturn(DebugLevel.ERROR, string.Format("Exception while reading commands from incoming data: {0}", ex));
                }
                SupportClass.WriteStackTrace(ex, null);
            }
        }

        //<summary>
        // Class to handle Http based connections to a Photon server.
        // Requests are done asynchronous and not queued at all.

        // All responses are put into the game's thread-context and 
        // all results and state changes are done within calls of
        // Service() or DispatchIncomingCommands().
        // </summary>
        internal NCommand RemoveSentReliableCommand(int ackReceivedReliableSequenceNumber, int ackReceivedChannel)
        {
            NCommand found = null;
            lock (this.sentReliableCommands)
            {
                foreach (NCommand result in this.sentReliableCommands)
                {
                    if (((result != null) && (result.reliableSequenceNumber == ackReceivedReliableSequenceNumber)) && (result.commandChannelID == ackReceivedChannel))
                    {
                        found = result;
                        break;
                    }
                }
                if (found != null)
                {
                    this.sentReliableCommands.Remove(found);
                    if (this.sentReliableCommands.Count > 0)
                    {
                        base.timeoutInt = this.sentReliableCommands[0].commandSentTime + this.sentReliableCommands[0].roundTripTimeout;
                    }
                    return found;
                }
                if (((base.debugOut >= DebugLevel.ALL) && (base.peerConnectionState != PeerBase.ConnectionStateValue.Connected)) && (base.peerConnectionState != PeerBase.ConnectionStateValue.Disconnecting))
                {
                    base.Listener.DebugReturn(DebugLevel.ALL, string.Format("No sent command for ACK (Ch: {0} Sq#: {1}). PeerState: {2}.", ackReceivedReliableSequenceNumber, ackReceivedChannel, base.peerConnectionState));
                }
            }
            return found;
        }

        /// <summary>
        /// gathers acks until udp-packet is full and sends it!
        /// </summary>
        /// <returns></returns>
        internal override bool SendAcksOnly()
        {
            if (base.peerConnectionState == PeerBase.ConnectionStateValue.Disconnected)
            {
                return false;
            }
            if (!this.rt.isRunning)
            {
                return false;
            }
            lock (this.udpBuffer)
            {
                int remainingCommands = 0;
                this.udpBufferIndex = 12;
                if (base.crcEnabled)
                {
                    this.udpBufferIndex += 4;
                }
                this.udpCommandCount = 0;
                base.timeInt = SupportClass.GetTickCount() - base.timeBase;
                lock (this.outgoingAcknowledgementsList)
                {
                    if (this.outgoingAcknowledgementsList.Count > 0)
                    {
                        remainingCommands = this.SerializeToBuffer(this.outgoingAcknowledgementsList);
                    }
                }
                if ((base.timeInt > base.timeoutInt) && (this.sentReliableCommands.Count > 0))
                {
                    lock (this.sentReliableCommands)
                    {
                        foreach (NCommand command in this.sentReliableCommands)
                        {
                            if (((command != null) && (command.roundTripTimeout != 0)) && ((base.timeInt - command.commandSentTime) > command.roundTripTimeout))
                            {
                                command.commandSentCount = 1;
                                command.roundTripTimeout = 0;
                                command.timeoutTime = 0x7fffffff;
                                command.commandSentTime = base.timeInt;
                            }
                        }
                    }
                }
                if (this.udpCommandCount <= 0)
                {
                    return false;
                }
                if (base.TrafficStatsEnabled)
                {
                    base.TrafficStatsOutgoing.TotalPacketCount++;
                    base.TrafficStatsOutgoing.TotalCommandsInPackets += this.udpCommandCount;
                }
                this.SendData(this.udpBuffer, this.udpBufferIndex);
                return (remainingCommands > 0);
            }
        }

        internal void SendData(byte[] data, int length)
        {
            try
            {
                int offset = 0;
                Protocol.Serialize(base.peerID, data, ref offset);
                data[2] = base.crcEnabled ? ((byte)0xcc) : ((byte)0);
                data[3] = this.udpCommandCount;
                offset = 4;
                Protocol.Serialize(base.timeInt, data, ref offset);
                Protocol.Serialize(this.challenge, data, ref offset);
                if (base.crcEnabled)
                {
                    Protocol.Serialize(0, data, ref offset);
                    uint crcValue = SupportClass.CalculateCrc(data, length);
                    offset -= 4;
                    Protocol.Serialize((int)crcValue, data, ref offset);
                }
                base.bytesOut += length;
                if (base.NetworkSimulationSettings.IsSimulationEnabled)
                {
                    byte[] dataCopy = new byte[length];
                    Buffer.BlockCopy(data, 0, dataCopy, 0, length);
                    base.SendNetworkSimulated(delegate
                    {
                        this.rt.SendUdpPackage(dataCopy, length);
                    });
                }
                else
                {
                    this.rt.SendUdpPackage(data, length);
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
            if (base.peerConnectionState == PeerBase.ConnectionStateValue.Disconnected)
            {
                return false;
            }
            if (!this.rt.isRunning)
            {
                return false;
            }
            lock (this.udpBuffer)
            {
                NCommand command;
                int remainingCommands = 0;
                this.udpBufferIndex = 12;
                if (base.crcEnabled)
                {
                    this.udpBufferIndex += 4;
                }
                this.udpCommandCount = 0;
                base.timeInt = SupportClass.GetTickCount() - base.timeBase;
                lock (this.outgoingAcknowledgementsList)
                {
                    if (this.outgoingAcknowledgementsList.Count > 0)
                    {
                        remainingCommands = this.SerializeToBuffer(this.outgoingAcknowledgementsList);
                    }
                }
                if ((!base.IsSendingOnlyAcks && (base.timeInt > base.timeoutInt)) && (this.sentReliableCommands.Count > 0))
                {
                    lock (this.sentReliableCommands)
                    {
                        Queue<NCommand> commandsToResend = new Queue<NCommand>();
                        foreach (NCommand command2 in this.sentReliableCommands)
                        {

                            command = command2;
                            if ((command != null) && ((base.timeInt - command.commandSentTime) > command.roundTripTimeout))
                            {
                                if ((command.commandSentCount > base.sentCountAllowance) || (base.timeInt > command.timeoutTime))
                                {
                                    if (base.debugOut >= DebugLevel.INFO)
                                    {
                                        base.Listener.DebugReturn(DebugLevel.INFO, string.Concat(new object[] { "Timeout-disconnect! Command: ", command, " now: ", base.timeInt, " challenge: ", Convert.ToString(this.challenge, 0x10) }));
                                    }
                                    base.Listener.OnStatusChanged(StatusCode.TimeoutDisconnect);
                                    base.peerConnectionState = PeerBase.ConnectionStateValue.Zombie;
                                    this.Disconnect();
                                    return false;
                                }
                                commandsToResend.Enqueue(command);
                            }

                        }
                        while (commandsToResend.Count > 0)
                        {
                            command = commandsToResend.Dequeue();
                            this.QueueOutgoingReliableCommand(command);
                            this.sentReliableCommands.Remove(command);
                            this.reliableCommandsRepeated++;
                            if (base.debugOut >= DebugLevel.INFO)
                            {
                                base.Listener.DebugReturn(DebugLevel.INFO, string.Format("Resending command: {0}. times out: {1} now: {2}", command, command.roundTripTimeout, base.timeInt));
                            }
                        }
                    }
                }
                if ((((!base.IsSendingOnlyAcks && (base.peerConnectionState == PeerBase.ConnectionStateValue.Connected)) && ((base.timePingInterval > 0) && (this.sentReliableCommands.Count == 0))) && (((base.timeInt - base.timeLastAckReceive) > base.timePingInterval) && !this.AreReliableCommandsInTransit())) && ((this.udpBufferIndex + 12) < this.udpBuffer.Length))
                {
                    command = new NCommand(this, 5, null, 0xff);
                    this.QueueOutgoingReliableCommand(command);
                    if (base.TrafficStatsEnabled)
                    {
                        base.TrafficStatsOutgoing.CountControlCommand(command.Size);
                    }
                }
                if (!base.IsSendingOnlyAcks)
                {
                    lock (this.channels)
                    {
                        foreach (EnetChannel channel in this.channels.Values)
                        {
                            remainingCommands += this.SerializeToBuffer(channel.outgoingReliableCommandsList);
                            remainingCommands += this.SerializeToBuffer(channel.outgoingUnreliableCommandsList);
                        }
                    }
                }
                if (this.udpCommandCount <= 0)
                {
                    return false;
                }
                if (base.TrafficStatsEnabled)
                {
                    base.TrafficStatsOutgoing.TotalPacketCount++;
                    base.TrafficStatsOutgoing.TotalCommandsInPackets += this.udpCommandCount;
                }
                this.SendData(this.udpBuffer, this.udpBufferIndex);
                return (remainingCommands > 0);
            }
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
            return fullMessageBytes;
        }

        internal int SerializeToBuffer(Queue<NCommand> commandList)
        {
            while (commandList.Count > 0)
            {
                NCommand command = commandList.Peek();
                if (command == null)
                {
                    commandList.Dequeue();
                }
                else
                {
                    if ((this.udpBufferIndex + command.Size) > this.udpBuffer.Length)
                    {
                        if (base.debugOut >= DebugLevel.INFO)
                        {
                            base.Listener.DebugReturn(DebugLevel.INFO, string.Concat(new object[] { "UDP package is full. Commands in Package: ", this.udpCommandCount, ". Commands left in queue: ", commandList.Count }));
                        }
                        break;
                    }
                    Buffer.BlockCopy(command.Serialize(), 0, this.udpBuffer, this.udpBufferIndex, command.Size);
                    this.udpBufferIndex += command.Size;
                    this.udpCommandCount = (byte)(this.udpCommandCount + 1);
                    if ((command.commandFlags & 1) > 0)
                    {
                        this.QueueSentCommand(command);
                    }
                    commandList.Dequeue();
                }
            }
            return commandList.Count;
        }

        internal override void StopConnection()
        {
            if (this.rt != null)
            {
                this.rt.StopConnection();
            }
        }

        // Properties
        internal override int QueuedIncomingCommandsCount
        {
            get
            {
                int x = 0;
                lock (this.channels)
                {
                    foreach (EnetChannel c in this.channels.Values)
                    {
                        x += c.incomingReliableCommandsList.Count;
                        x += c.incomingUnreliableCommandsList.Count;
                    }
                }
                return x;
            }
        }

        internal override int QueuedOutgoingCommandsCount
        {
            get
            {
                int x = 0;
                lock (this.channels)
                {
                    foreach (EnetChannel c in this.channels.Values)
                    {
                        x += c.outgoingReliableCommandsList.Count;
                        x += c.outgoingUnreliableCommandsList.Count;
                    }
                }
                return x;
            }
        }
    }
}
