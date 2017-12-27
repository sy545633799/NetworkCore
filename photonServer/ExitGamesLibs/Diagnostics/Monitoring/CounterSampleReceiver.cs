using System;
using System.IO;
using System.Threading;
using ExitGames.Concurrency.Channels;
using ExitGames.Logging;
using ExitGames.Net.Sockets;

namespace ExitGames.Diagnostics.Monitoring
{
    /// <summary>
    ///  A receiver of <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSamplesPackage"/>s.
    /// </summary>
    public class CounterSampleReceiver
    {
        /// <summary>
        /// Provides a <see cref="T:ExitGames.Logging.ILogger"/> instance used to log messages into the logging framework.
        /// </summary>
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// The <see cref="T:ExitGames.Net.Sockets.ISocketReceiver"/> used to receive data.
        /// </summary>
        private readonly ISocketReceiver receiver;

        /// <summary>
        /// A Channel for <see cref="T:ExitGames.Diagnostics.Monitoring.CounterSamplesPackage"/>s.
        /// </summary>
        public readonly Channel<CounterSamplesPackage> Channel = new Channel<CounterSamplesPackage>();

        /// <summary>
        /// This event is invoked when new <see 
        /// cref="T:ExitGames.Diagnostics.Monitoring.CounterSamplesPackage"/> are received.
        /// </summary>
        public event EventHandler<CounterSamplesPackage> OnCounterDataReceived;

        /// <summary>
        /// The <see cref="E:ExitGames.Net.Sockets.ISocketReceiver.Receive"/> callback.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event args.</param>
        private void OnReceive(object sender, SocketReceiveEventArgs e)
        {
            try
            {
                DateTime receiveTime = DateTime.UtcNow;
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Received data from {0}.", new object[] { e.RemoteEndPoint });
                }
                if ((OnCounterDataReceived != null) || Channel.HasSubscriptions)
                {
                    using (MemoryStream input = new MemoryStream(e.Buffer, e.Offset, e.BytesReceived))
                    {
                        BinaryReader binaryReader = new BinaryReader(input);
                        binaryReader.PeekChar();
                        long dateData = binaryReader.ReadInt64();
                        DateTime senderTime = DateTime.FromBinary(dateData);
                        int count = binaryReader.ReadInt32();
                        string senderId = binaryReader.ReadString();
                        CounterSampleCollection[] counterSamples = new CounterSampleCollection[count];
                        for (int i = 0; i < count; i++)
                        {
                            counterSamples[i] = CounterSampleCollection.Deserialize(binaryReader);
                        }
                        CounterSamplesPackage counterSamle = new CounterSamplesPackage(e.RemoteEndPoint, senderId, senderTime, receiveTime, counterSamples);
                        if (OnCounterDataReceived != null)
                        {
                            OnCounterDataReceived(this, counterSamle);
                        }
                        Channel.Publish(counterSamle);
                    }
                }
            }
            catch (ThreadAbortException)
            {
                throw;
            }
            catch (OutOfMemoryException)
            {
                throw;
            }
            catch (Exception exception)
            {
                if (log.IsErrorEnabled)
                {
                    log.Error("Error during receive.", exception);
                }
            }
        }

        /// <summary>
        ///  Initializes a new instance of the <see 
        ///  cref="T:ExitGames.Diagnostics.Monitoring.CounterSampleReceiver"/> class.
        /// </summary>
        /// <param name="receiver">The receiver.</param>
        public CounterSampleReceiver(ISocketReceiver receiver)
        {
            this.receiver = receiver;
            this.receiver.Receive += new EventHandler<SocketReceiveEventArgs>(OnReceive);
        }
    }

}
