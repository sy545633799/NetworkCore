using System;
using System.Net;

namespace ExitGames.Diagnostics.Monitoring
{
    /// <summary>
    /// Argument for event <see cref="E:ExitGames.Diagnostics.Monitoring.CounterSampleReceiver.OnCounterDataReceived"/> and message type for <see
    /// cref="F:ExitGames.Diagnostics.Monitoring.CounterSampleReceiver.Channel"/> 
    /// </summary>
    public class CounterSamplesPackage : EventArgs
    {
        /// <summary>
        /// The samples.
        /// </summary>
        private readonly CounterSampleCollection[] counterSamples;

        /// <summary>
        /// The receive time.
        /// </summary>
        private readonly DateTime receiveTime;

        /// <summary>
        /// The remote end point.
        /// </summary>
        private readonly EndPoint remoteEndPoint;

        /// <summary>
        /// The sender id.
        /// </summary>
        private readonly string senderId;

        /// <summary>
        /// The send time.
        /// </summary>
        private readonly DateTime senderTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSamplesPackage"/> class.
        /// </summary>
        /// <param name="remoteEndPoint">The remote end point.</param>
        /// <param name="senderId">The sender id.</param>
        /// <param name="senderTime">The sender time.</param>
        /// <param name="receiveTime">The receive time.</param>
        /// <param name="counterSamples">The counter samples.</param>
        public CounterSamplesPackage(EndPoint remoteEndPoint, string senderId, DateTime senderTime, DateTime receiveTime, CounterSampleCollection[] counterSamples)
        {
            this.remoteEndPoint = remoteEndPoint;
            this.senderId = senderId;
            this.senderTime = senderTime;
            this.receiveTime = receiveTime;
            this.counterSamples = counterSamples;
        }

        /// <summary>
        /// Gets the received <see cref="T:ExitGames.Diagnostics.Counter.CounterSample"/>s.
        /// </summary>
        public CounterSampleCollection[] CounterSamples
        {
            get
            {
                return this.counterSamples;
            }
        }

        /// <summary>
        /// Gets the time when counters were received.
        /// </summary>
        public DateTime ReceiveTime
        {
            get
            {
                return this.receiveTime;
            }
        }

        /// <summary>
        /// Gets the remote end point.
        /// </summary>
        public EndPoint RemoteEndPoint
        {
            get
            {
                return this.remoteEndPoint;
            }
        }

        /// <summary>
        /// Gets the sender's id.
        /// </summary>
        public string SenderId
        {
            get
            {
                return this.senderId;
            }
        }

        /// <summary>
        /// Gets the send time.
        /// </summary>
        public DateTime SenderTime
        {
            get
            {
                return this.senderTime;
            }
        }
    }
}
