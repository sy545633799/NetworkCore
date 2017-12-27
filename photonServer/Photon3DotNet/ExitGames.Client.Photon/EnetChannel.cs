namespace ExitGames.Client.Photon
{
    using System.Collections.Generic;

    internal class EnetChannel
    {
        internal byte ChannelNumber;
        internal Dictionary<int, NCommand> incomingReliableCommandsList;
        internal int incomingReliableSequenceNumber;
        internal Dictionary<int, NCommand> incomingUnreliableCommandsList;
        internal int incomingUnreliableSequenceNumber;
        internal Queue<NCommand> outgoingReliableCommandsList;
        internal int outgoingReliableSequenceNumber;
        internal Queue<NCommand> outgoingUnreliableCommandsList;
        internal int outgoingUnreliableSequenceNumber;

        public EnetChannel(byte channelNumber, int commandBufferSize)
        {
            this.ChannelNumber = channelNumber;
            this.incomingReliableCommandsList = new Dictionary<int, NCommand>(commandBufferSize);
            this.incomingUnreliableCommandsList = new Dictionary<int, NCommand>(commandBufferSize);
            this.outgoingReliableCommandsList = new Queue<NCommand>(commandBufferSize);
            this.outgoingUnreliableCommandsList = new Queue<NCommand>(commandBufferSize);
        }

        public void clearAll()
        {
            lock (this)
            {
                this.incomingReliableCommandsList.Clear();
                this.incomingUnreliableCommandsList.Clear();
                this.outgoingReliableCommandsList.Clear();
                this.outgoingUnreliableCommandsList.Clear();
            }
        }

        public bool ContainsReliableSequenceNumber(int reliableSequenceNumber)
        {
            return this.incomingReliableCommandsList.ContainsKey(reliableSequenceNumber);
        }

        public bool ContainsUnreliableSequenceNumber(int unreliableSequenceNumber)
        {
            return this.incomingUnreliableCommandsList.ContainsKey(unreliableSequenceNumber);
        }

        public NCommand FetchReliableSequenceNumber(int reliableSequenceNumber)
        {
            return this.incomingReliableCommandsList[reliableSequenceNumber];
        }

        public NCommand FetchUnreliableSequenceNumber(int unreliableSequenceNumber)
        {
            return this.incomingUnreliableCommandsList[unreliableSequenceNumber];
        }
    }
}
