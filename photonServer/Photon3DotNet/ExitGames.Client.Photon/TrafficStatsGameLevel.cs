
namespace ExitGames.Client.Photon
{
    using System;

    /// <summary>
    /// Only in use as long as PhotonPeer.TrafficStatsEnabled = true;
    /// </summary>
    public class TrafficStatsGameLevel
    {
        private int timeOfLastDispatchCall;

        private int timeOfLastSendCall;

        /// <summary>
        /// Gets sum of outgoing operations in bytes.
        /// </summary>
        public int OperationByteCount
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets count of outgoing operations.
        /// </summary>
        public int OperationCount
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets sum of byte-cost of incoming operation-results.
        /// </summary>
        public int ResultByteCount
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets count of incoming operation-results.
        /// </summary>
        public int ResultCount
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets sum of byte-cost of incoming events.
        /// </summary>
        public int EventByteCount
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets count of incoming events.
        /// </summary>
        public int EventCount
        {
            get;
            internal set;
        }

        ///<summary>
        /// Gets longest time it took to complete a call to OnOperationResponse (in your code).
        /// If such a callback takes long, it will lower the network performance and might lead to timeouts.
        ///</summary>
        public int LongestOpResponseCallback
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets OperationCode that causes the LongestOpResponseCallback. See that description.
        /// </summary>
        public byte LongestOpResponseCallbackOpCode
        {
            get;
            internal set;
        }

        ///<summary>
        /// Gets longest time a call to OnEvent (in your code) took.
        /// If such a callback takes long, it will lower the network performance and might lead to timeouts.
        ///</summary>      
        public int LongestEventCallback
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets EventCode that caused the LongestEventCallback. See that description.
        /// </summary>
        public byte LongestEventCallbackCode
        {
            get;
            internal set;
        }

        ///<summary>
        /// Gets longest time between subsequent calls to DispatchIncomginCommands in milliseconds.
        /// Note: This is not a crucial timing for the networking. Long gaps just add "local lag" to events that are available already.
        ///</summary>
        public int LongestDeltaBetweenDispatching
        {
            get;
            internal set;
        }

        ///<summary>
        /// Gets longest time between subsequent calls to SendOutgoingCommands in milliseconds.
        /// Note: This is a crucial value for network stability. Without calling SendOutgoingCommands,
        /// nothing will be sent to the server, who might time out this client.
        /// </summary>
        public int LongestDeltaBetweenSending
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets number of calls of DispatchIncomingCommands.
        /// </summary>
        [Obsolete("Use DispatchIncomingCommandsCalls, which has proper naming.")]
        public int DispatchCalls
        {
            get
            {
                return this.DispatchIncomingCommandsCalls;
            }
        }

        /// <summary>
        /// Gets number of calls of SendOutgoingCommands.
        /// </summary>
        public int DispatchIncomingCommandsCalls
        {
            get;

            internal set;
        }

        /// <summary>
        /// Gets sum of byte-cost of all "logic level" messages.
        /// </summary>
        public int TotalByteCount
        {
            get
            {
                return ((this.OperationByteCount + this.ResultByteCount) + this.EventByteCount);
            }
        }

        /// <summary>
        /// Gets sum of counted "logic level" messages.
        /// </summary>
        public int TotalMessageCount
        {
            get
            {
                return ((this.OperationCount + this.ResultCount) + this.EventCount);
            }
        }

        /// <summary>
        /// Gets sum of byte-cost of all incoming "logic level" messages.
        /// </summary>
        public int TotalIncomingByteCount
        {
            get
            {
                return (this.ResultByteCount + this.EventByteCount);
            }
        }

        /// <summary>
        /// Gets sum of counted incoming "logic level" messages.
        /// </summary>
        public int TotalIncomingMessageCount
        {
            get
            {
                return (this.ResultCount + this.EventCount);
            }
        }

        /// <summary>
        /// Gets sum of byte-cost of all outgoing "logic level" messages (= OperationByteCount).
        /// </summary>
        public int TotalOutgoingByteCount
        {
            get
            {
                return this.OperationByteCount;
            }
        }

        /// <summary>
        /// Gets sum of counted outgoing "logic level" messages (= OperationCount).
        /// </summary>
        public int TotalOutgoingMessageCount
        {
            get
            {
                return this.OperationCount;
            }
        }

        internal void CountEvent(int eventBytes)
        {
            this.EventByteCount += eventBytes;
            this.EventCount++;
        }

        internal void CountOperation(int operationBytes)
        {
            this.OperationByteCount += operationBytes;
            this.OperationCount++;
        }

        internal void CountResult(int resultBytes)
        {
            this.ResultByteCount += resultBytes;
            this.ResultCount++;
        }

        internal void DispatchIncomingCommandsCalled()
        {
            if (this.timeOfLastDispatchCall != 0)
            {
                int delta = SupportClass.GetTickCount() - this.timeOfLastDispatchCall;
                if (delta > this.LongestDeltaBetweenDispatching)
                {
                    this.LongestDeltaBetweenDispatching = delta;
                }
            }
            this.DispatchIncomingCommandsCalls++;
            this.timeOfLastDispatchCall = SupportClass.GetTickCount();
        }

        public void ResetMaximumCounters()
        {
            this.LongestDeltaBetweenDispatching = 0;
            this.LongestDeltaBetweenSending = 0;
            this.LongestEventCallback = 0;
            this.LongestEventCallbackCode = 0;
            this.LongestOpResponseCallback = 0;
            this.LongestOpResponseCallbackOpCode = 0;
            this.timeOfLastDispatchCall = 0;
            this.timeOfLastSendCall = 0;
        }

        internal void SendOutgoingCommandsCalled()
        {
            if (this.timeOfLastSendCall != 0)
            {
                int delta = SupportClass.GetTickCount() - this.timeOfLastSendCall;
                if (delta > this.LongestDeltaBetweenSending)
                {
                    this.LongestDeltaBetweenSending = delta;
                }
            }
            this.SendOutgoingCommandsCalls++;
            this.timeOfLastSendCall = SupportClass.GetTickCount();
        }

        internal void TimeForEventCallback(byte code, int time)
        {
            if (time > this.LongestOpResponseCallback)
            {
                this.LongestEventCallback = time;
                this.LongestEventCallbackCode = code;
            }
        }

        internal void TimeForResponseCallback(byte code, int time)
        {
            if (time > this.LongestOpResponseCallback)
            {
                this.LongestOpResponseCallback = time;
                this.LongestOpResponseCallbackOpCode = code;
            }
        }

        public override string ToString()
        {
            return string.Format("OperationByteCount: {0} ResultByteCount: {1} EventByteCount: {2}", this.OperationByteCount, this.ResultByteCount, this.EventByteCount);
        }

        public string ToStringVitalStats()
        {
            return string.Format("Longest delta between Send: {0}ms Dispatch: {1}ms. Longest callback OnEv: {3}={2}ms OnResp: {5}={4}ms. Calls of Send: {6} Dispatch: {7}.", new object[] { this.LongestDeltaBetweenSending, this.LongestDeltaBetweenDispatching, this.LongestEventCallback, this.LongestEventCallbackCode, this.LongestOpResponseCallback, this.LongestOpResponseCallbackOpCode, this.SendOutgoingCommandsCalls, this.DispatchIncomingCommandsCalls });
        }

        public int SendOutgoingCommandsCalls
        {
            get;
            internal set;
        }
    }
}
