namespace ExitGames.Client.Photon
{
    using System;

    /// <summary>
    /// Internal class for "commands" - the package in which operations are sent.
    /// </summary>
    internal class NCommand : IComparable<NCommand>
    {
        internal int ackReceivedReliableSequenceNumber;
        internal int ackReceivedSentTime;
        internal const int CmdSizeAck = 20;
        internal const int CmdSizeConnect = 0x2c;
        internal const int CmdSizeDisconnect = 12;
        internal const int CmdSizeFragmentHeader = 0x20;
        internal const int CmdSizeMinimum = 12;
        internal const int CmdSizePing = 12;
        internal const int CmdSizeReliableHeader = 12;
        internal const int CmdSizeUnreliableHeader = 0x10;
        internal const int CmdSizeVerifyConnect = 0x2c;
        internal byte commandChannelID;
        internal byte commandFlags;
        internal byte commandSentCount;
        internal int commandSentTime;
        internal byte commandType;
        private byte[] completeCommand;
        internal const byte CT_ACK = 1;
        internal const byte CT_CONNECT = 2;
        internal const byte CT_DISCONNECT = 4;
        internal const byte CT_EG_SERVERTIME = 12;
        internal const byte CT_NONE = 0;
        internal const byte CT_PING = 5;
        internal const byte CT_SENDFRAGMENT = 8;
        internal const byte CT_SENDRELIABLE = 6;
        internal const byte CT_SENDUNRELIABLE = 7;
        internal const byte CT_VERIFYCONNECT = 3;
        internal const int FLAG_RELIABLE = 1;
        internal const int FLAG_UNSEQUENCED = 2;
        internal int fragmentCount;
        internal int fragmentNumber;
        internal int fragmentOffset;
        internal int fragmentsRemaining;
        internal const byte FV_RELIABLE = 1;
        internal const byte FV_UNRELIABLE = 0;
        internal const byte FV_UNRELIBALE_UNSEQUENCED = 2;
        internal const int HEADER_FRAGMENT_LENGTH = 0x20;
        internal const int HEADER_LENGTH = 12;
        internal const int HEADER_UDP_PACK_LENGTH = 12;
        internal byte[] Payload;
        internal int reliableSequenceNumber;
        internal byte reservedByte;
        internal int roundTripTimeout;
        internal int Size;
        internal int startSequenceNumber;
        internal int timeoutTime;
        internal int totalLength;
        internal int unreliableSequenceNumber;
        internal int unsequencedGroupNumber;

        /// <summary>
        /// reads the command values (commandHeader and command-values) from incoming bytestream and populates the incoming command*
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="inBuff"></param>
        /// <param name="readingOffset"></param>
        internal NCommand(EnetPeer peer, byte[] inBuff, ref int readingOffset)
        {
            this.commandType = inBuff[readingOffset++];
            this.commandChannelID = inBuff[readingOffset++];
            this.commandFlags = inBuff[readingOffset++];
            this.reservedByte = inBuff[readingOffset++];
            Protocol.Deserialize(out this.Size, inBuff, ref readingOffset);
            Protocol.Deserialize(out this.reliableSequenceNumber, inBuff, ref readingOffset);
            peer.bytesIn += this.Size;
            switch (this.commandType)
            {
                case 1:
                    Protocol.Deserialize(out this.ackReceivedReliableSequenceNumber, inBuff, ref readingOffset);
                    Protocol.Deserialize(out this.ackReceivedSentTime, inBuff, ref readingOffset);
                    break;

                case 3:
                    short outgoingPeerID;
                    Protocol.Deserialize(out outgoingPeerID, inBuff, ref readingOffset);
                    readingOffset += 30;
                    if (peer.peerID == -1)
                    {
                        peer.peerID = outgoingPeerID;
                    }
                    break;

                case 6:
                    this.Payload = new byte[this.Size - 12];
                    break;

                case 7:
                    Protocol.Deserialize(out this.unreliableSequenceNumber, inBuff, ref readingOffset);
                    this.Payload = new byte[this.Size - 0x10];
                    break;

                case 8:
                    Protocol.Deserialize(out this.startSequenceNumber, inBuff, ref readingOffset);
                    Protocol.Deserialize(out this.fragmentCount, inBuff, ref readingOffset);
                    Protocol.Deserialize(out this.fragmentNumber, inBuff, ref readingOffset);
                    Protocol.Deserialize(out this.totalLength, inBuff, ref readingOffset);
                    Protocol.Deserialize(out this.fragmentOffset, inBuff, ref readingOffset);
                    this.Payload = new byte[this.Size - 0x20];
                    this.fragmentsRemaining = this.fragmentCount;
                    break;
            }
            if (this.Payload != null)
            {
                Buffer.BlockCopy(inBuff, readingOffset, this.Payload, 0, this.Payload.Length);
                readingOffset += this.Payload.Length;
            }
        }

        /// <summary>
        /// this variant does only create outgoing commands and increments . incoming ones are created from a DataInputStream
        /// </summary>
        /// <param name="peer"></param>
        /// <param name="commandType"></param>
        /// <param name="payload"></param>
        /// <param name="channel"></param>
        internal NCommand(EnetPeer peer, byte commandType, byte[] payload, byte channel)
        {
            this.commandType = commandType;
            this.commandFlags = 1;
            this.commandChannelID = channel;
            this.Payload = payload;
            this.Size = 12;
            switch (this.commandType)
            {
                case 1:
                    this.Size = 20;
                    this.commandFlags = 0;
                    break;

                case 2:
                    {
                        this.Size = 0x2c;
                        this.Payload = new byte[0x20];
                        this.Payload[0] = 0;
                        this.Payload[1] = 0;
                        int mtuOffset = 2;
                        Protocol.Serialize((short)peer.mtu, this.Payload, ref mtuOffset);
                        this.Payload[4] = 0;
                        this.Payload[5] = 0;
                        this.Payload[6] = 0x80;
                        this.Payload[7] = 0;
                        this.Payload[11] = peer.ChannelCount;
                        this.Payload[15] = 0;
                        this.Payload[0x13] = 0;
                        this.Payload[0x16] = 0x13;
                        this.Payload[0x17] = 0x88;
                        this.Payload[0x1b] = 2;
                        this.Payload[0x1f] = 2;
                        break;
                    }
                case 4:
                    this.Size = 12;
                    if (peer.peerConnectionState != PeerBase.ConnectionStateValue.Connected)
                    {
                        this.commandFlags = 2;
                    }
                    break;

                case 6:
                    this.Size = 12 + payload.Length;
                    break;

                case 7:
                    this.Size = 0x10 + payload.Length;
                    this.commandFlags = 0;
                    break;

                case 8:
                    this.Size = 0x20 + payload.Length;
                    break;
            }
        }

        public int CompareTo(NCommand other)
        {
            if ((this.commandFlags & 1) != 0)
            {
                return (this.reliableSequenceNumber - other.reliableSequenceNumber);
            }
            return (this.unreliableSequenceNumber - other.unreliableSequenceNumber);
        }

        internal static NCommand CreateAck(EnetPeer peer, NCommand commandToAck, int sentTime)
        {
            byte[] payload = new byte[8];
            int offset = 0;
            Protocol.Serialize(commandToAck.reliableSequenceNumber, payload, ref offset);
            Protocol.Serialize(sentTime, payload, ref offset);
            return new NCommand(peer, 1, payload, commandToAck.commandChannelID);
        }

        internal byte[] Serialize()
        {
            if (this.completeCommand == null)
            {
                int payloadLength = (this.Payload == null) ? 0 : this.Payload.Length;
                int headerLength = 12;
                if (this.commandType == 7)
                {
                    headerLength = 0x10;
                }
                else if (this.commandType == 8)
                {
                    headerLength = 0x20;
                }
                this.completeCommand = new byte[headerLength + payloadLength];
                this.completeCommand[0] = this.commandType;
                this.completeCommand[1] = this.commandChannelID;
                this.completeCommand[2] = this.commandFlags;
                this.completeCommand[3] = 4;
                int offset = 4;
                Protocol.Serialize(this.completeCommand.Length, this.completeCommand, ref offset);
                Protocol.Serialize(this.reliableSequenceNumber, this.completeCommand, ref offset);
                if (this.commandType == 7)
                {
                    offset = 12;
                    Protocol.Serialize(this.unreliableSequenceNumber, this.completeCommand, ref offset);
                }
                else if (this.commandType == 8)
                {
                    offset = 12;
                    Protocol.Serialize(this.startSequenceNumber, this.completeCommand, ref offset);
                    Protocol.Serialize(this.fragmentCount, this.completeCommand, ref offset);
                    Protocol.Serialize(this.fragmentNumber, this.completeCommand, ref offset);
                    Protocol.Serialize(this.totalLength, this.completeCommand, ref offset);
                    Protocol.Serialize(this.fragmentOffset, this.completeCommand, ref offset);
                }
                if (payloadLength > 0)
                {
                    Buffer.BlockCopy(this.Payload, 0, this.completeCommand, headerLength, payloadLength);
                }
                this.Payload = null;
            }
            return this.completeCommand;
        }

        public override string ToString()
        {
            return string.Format("NC({0}|{1} r/u: {2}/{3} st/r#/rt:{4}/{5}/{6})",   this.commandChannelID, this.commandType, this.reliableSequenceNumber, this.unreliableSequenceNumber, this.commandSentTime, this.commandSentCount, this.timeoutTime );
        }
    }
}
