namespace ExitGames.Client.Photon
{
    public class TrafficStats
    {
        internal TrafficStats(int packageHeaderSize)
        {
            this.PackageHeaderSize = packageHeaderSize;
        }

        internal void CountControlCommand(int size)
        {
            this.ControlCommandBytes += size;
            this.ControlCommandCount++;
        }

        internal void CountFragmentOpCommand(int size)
        {
            this.FragmentCommandBytes += size;
            this.FragmentCommandCount++;
        }

        internal void CountReliableOpCommand(int size)
        {
            this.ReliableCommandBytes += size;
            this.ReliableCommandCount++;
        }

        internal void CountUnreliableOpCommand(int size)
        {
            this.UnreliableCommandBytes += size;
            this.UnreliableCommandCount++;
        }

        public override string ToString()
        {
            return string.Format("TotalPacketBytes: {0} TotalCommandBytes: {1} TotalPacketCount: {2} TotalCommandsInPackets: {3}", this.TotalPacketBytes, this.TotalCommandBytes, this.TotalPacketCount, this.TotalCommandsInPackets);
        }

        public int ControlCommandBytes
        {
            get;
            internal set;
        }

        public int ControlCommandCount
        {
            get;
            internal set;
        }

        public int FragmentCommandBytes
        {
            get;
            internal set;
        }

        public int FragmentCommandCount
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the byte-size of per-package headers.
        /// </summary>
        public int PackageHeaderSize
        {
            get;
            internal set;
        }

        public int ReliableCommandBytes
        {
            get;
            internal set;
        }

        public int ReliableCommandCount
        {
            get;
            internal set;
        }

        public int TotalCommandBytes
        {
            get
            {
                return this.ReliableCommandBytes + this.UnreliableCommandBytes + this.FragmentCommandBytes + this.ControlCommandBytes;
            }
        }

        public int TotalCommandCount
        {
            get
            {
                return this.ReliableCommandCount + this.UnreliableCommandCount + this.FragmentCommandCount + this.ControlCommandCount;
            }
        }

        public int TotalCommandsInPackets
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets count of bytes as traffic, excluding UDP/TCP headers (42 bytes / x bytes).
        /// </summary>
        public int TotalPacketBytes
        {
            get
            {
                return this.TotalCommandBytes + this.TotalPacketCount * this.PackageHeaderSize;
            }
        }

        public int TotalPacketCount
        {
            get;
            internal set;
        }

        public int UnreliableCommandBytes
        {
            get;
            internal set;
        }

        public int UnreliableCommandCount
        {
            get;
            internal set;
        }
    }
}
