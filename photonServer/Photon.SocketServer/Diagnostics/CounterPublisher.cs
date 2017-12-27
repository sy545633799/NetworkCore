using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using ExitGames.Diagnostics.Counter;
using ExitGames.Diagnostics.Monitoring;
using ExitGames.Logging;
using ExitGames.Net.Sockets;
using ExitGames.Net.Sockets.Http;
using ExitGames.Net.Sockets.Pgm;
using ExitGames.Net.Sockets.Udp;
using Photon.SocketServer.Diagnostics.Configuration;

namespace Photon.SocketServer.Diagnostics
{
    public sealed class CounterPublisher
    {
        // Fields
        private readonly string address;
        private readonly CounterSamplePublisher counterPublisher;
        private CounterSampleSender counterSender;
        private IDisposable counterSenderSubsciption;
        private static CounterPublisher defaultInstance;
        private readonly bool enabled;
        private readonly IPEndPoint endPoint;
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private readonly int maxItemsPerRequest;
        private readonly int maxRequestsInQueue;
        private readonly ProtocolType protocol;
        private bool running;
        private readonly string senderId;
        private readonly IPAddress sendInterface;
        private readonly int sendInterval;
        private ISocketSender socketSender;
        private static readonly object syncRoot = new object();
        private readonly int updateInterval;

        // Methods
        public CounterPublisher(CounterPublisherSettings settings)
        {
            this.maxItemsPerRequest = 0x3e8;
            this.maxRequestsInQueue = 120;
            this.senderId = Environment.MachineName;
            this.enabled = settings.Enabled;
            this.sendInterval = settings.PublishInterval;
            this.updateInterval = settings.UpdateInterval;
            this.maxItemsPerRequest = settings.MaxCounterPerRequest;
            this.maxRequestsInQueue = settings.MaxQueueLength;
            if (!string.IsNullOrEmpty(settings.SenderId))
            {
                this.senderId = settings.SenderId.Replace("{0}", Environment.MachineName);
            }
            this.counterPublisher = new CounterSamplePublisher(this.updateInterval);
            if (this.enabled)
            {
                switch (settings.Protocol.ToLower())
                {
                    case "udp":
                        this.protocol = ProtocolType.Udp;
                        break;

                    case "http":
                        this.protocol = ProtocolType.Http;
                        break;

                    case "pgm":
                        this.protocol = ProtocolType.Pgm;
                        break;

                    default:
                        throw new ArgumentException("Invalid protocol specified. Only Udp and Pgm protocols are supported", "settings");
                }
                this.address = settings.Endpoint;
                if ((this.protocol != ProtocolType.Http) && !settings.TryGetIpEndpoint(out this.endPoint))
                {
                    if (log.IsWarnEnabled)
                    {
                        log.WarnFormat("Invalid CounterPublisher endpoint specified: {0}", new object[] { settings.Endpoint });
                    }
                    this.enabled = false;
                }
                if (this.protocol == ProtocolType.Pgm)
                {
                    IPAddress address;
                    if (!settings.TryGetSendInterface(out address))
                    {
                        if (log.IsWarnEnabled)
                        {
                            log.WarnFormat("Invalid CounterPublisher SendInterface specified: {0}", new object[] { settings.SendInterface });
                        }
                        this.enabled = false;
                    }
                    this.sendInterface = address;
                }
            }
        }

        public CounterPublisher(IPEndPoint endPoint, int updateInterval, int sendInterval)
            : this(ProtocolType.Udp, endPoint, updateInterval, sendInterval)
        {
        }

        public CounterPublisher(ProtocolType protocol, IPEndPoint endPoint, int updateInterval, int sendInterval)
            : this(protocol, endPoint, null, updateInterval, sendInterval)
        {
        }

        public CounterPublisher(IPEndPoint endPoint, IPAddress sendInterface, int updateInterval, int sendInterval)
            : this(ProtocolType.Udp, endPoint, sendInterface, updateInterval, sendInterval)
        {
        }

        public CounterPublisher(ProtocolType protocol, IPEndPoint endPoint, IPAddress sendInterface, int updateInterval, int sendInterval)
        {
            this.maxItemsPerRequest = 0x3e8;
            this.maxRequestsInQueue = 120;
            this.senderId = Environment.MachineName;
            this.address = endPoint.ToString();
            this.endPoint = endPoint;
            this.protocol = protocol;
            this.sendInterface = sendInterface;
            this.sendInterval = sendInterval;
            this.updateInterval = updateInterval;
            this.enabled = true;
            this.counterPublisher = new CounterSamplePublisher(updateInterval);
        }

        public void AddCounter(ICounter counter, string name)
        {
            this.counterPublisher.AddCounter(counter, name);
        }

        public void AddCounterClass(object counterClass)
        {
            this.AddCounterClass(counterClass, string.Empty);
        }

        public void AddCounterClass(object counterClass, string nameSpace)
        {
            CounterSamplePublisherFactory.InitializeCounterPublisher(this.counterPublisher, counterClass, nameSpace);
        }

        public void AddStaticCounterClass(Type counterClass)
        {
            this.AddStaticCounterClass(counterClass, string.Empty);
        }

        public void AddStaticCounterClass(Type counterClass, string nameSpace)
        {
            CounterSamplePublisherFactory.InitializeCounterPublisher(this.counterPublisher, counterClass, nameSpace);
        }

        private void OnCounterSenderDisconnected(object sender, EventArgs e)
        {
            this.Stop();
            log.WarnFormat("CounterPublisher stopped working because of a network loss", new object[0]);
        }

        private void OnCounterSenderError(object sender, UnhandledExceptionEventArgs e)
        {
            SocketException exceptionObject = e.ExceptionObject as SocketException;
            if ((exceptionObject != null) && (exceptionObject.SocketErrorCode == SocketError.InvalidArgument))
            {
                this.Stop();
                log.WarnFormat("CounterPublisher stopped working because of a network loss", new object[0]);
            }
            else if (this.counterSender != null)
            {
                Exception exception2 = e.ExceptionObject as Exception;
                if ((this.counterSender.MaxRetryCount >= 0) && (this.counterSender.ErrorCount >= this.counterSender.MaxRetryCount))
                {
                    log.WarnFormat("CounterPublisher stopped working: {0}", new object[] { (exception2 == null) ? e.ExceptionObject : exception2.Message });
                    this.Stop();
                }
                else if (this.counterSender.MaxRetryCount == 1)
                {
                    if (this.counterSender.MaxRetryCount > 0)
                    {
                        log.WarnFormat("Failed to publish counter data. Counter publisher will retry publishing for {0} times: {0}", new object[] { this.counterSender.MaxRetryCount - 1, (exception2 == null) ? e.ExceptionObject : exception2.Message });
                    }
                    else
                    {
                        log.WarnFormat("Failed to publish counter data. Counter publisher will retry publishing: {0}", new object[] { (exception2 == null) ? e.ExceptionObject : exception2.Message });
                    }
                }
            }
        }

        public void Start()
        {
            lock (syncRoot)
            {
                if (this.running)
                {
                    return;
                }
                this.running = true;
            }
            if (this.enabled)
            {
                try
                {
                    if (this.protocol == ProtocolType.Pgm)
                    {
                        PgmSender sender;
                        if (!NetworkInterface.GetIsNetworkAvailable())
                        {
                            log.Warn("CounterPublisher could not started because no network connection is available.");
                            return;
                        }
                        try
                        {
                            sender = new PgmSender(this.endPoint);
                            if (this.sendInterface == null)
                            {
                                sender.Connect();
                            }
                            else
                            {
                                sender.Connect(this.sendInterface.ToString());
                            }
                        }
                        catch (Exception exception)
                        {
                            log.WarnFormat("CounterPublisher could not started. To publish counter values the PGM protocol must be installed.\r\n: Error={0}", new object[] { exception });
                            throw;
                        }
                        this.socketSender = sender;
                    }
                    else if (this.protocol == ProtocolType.Udp)
                    {
                        UdpSender sender2 = new UdpSender(this.endPoint);
                        sender2.Start();
                        this.socketSender = sender2;
                    }
                    else
                    {
                        this.socketSender = new HttpSender(this.address);
                    }
                    this.counterSender = new CounterSampleSender(this.senderId, this.sendInterval, this.socketSender, this.maxRequestsInQueue, -1, this.maxItemsPerRequest);
                    this.counterSenderSubsciption = this.counterSender.SubscribeToChannel(this.counterPublisher.Channel);
                    this.counterSender.OnDisconnected += new EventHandler(this.OnCounterSenderDisconnected);
                    this.counterSender.OnError += new EventHandler<UnhandledExceptionEventArgs>(this.OnCounterSenderError);
                    this.counterPublisher.Start();
                    this.counterSender.Start();
                    log.InfoFormat("CounterPublisher started on: {0}", new object[] { this.address });
                }
                catch (SocketException exception2)
                {
                    log.WarnFormat("CounterPublisher could not be created: ReturnCode={0}, Message={1}", new object[] { exception2.SocketErrorCode, exception2.Message });
                    throw;
                }
            }
        }

        private void Stop()
        {
            lock (syncRoot)
            {
                if (!this.running)
                {
                    return;
                }
                this.running = false;
            }
            if (this.counterSenderSubsciption != null)
            {
                this.counterSenderSubsciption.Dispose();
                this.counterSenderSubsciption = null;
            }
            if (this.counterSender != null)
            {
                this.counterSender.OnDisconnected -= new EventHandler(this.OnCounterSenderDisconnected);
                this.counterSender.OnError -= new EventHandler<UnhandledExceptionEventArgs>(this.OnCounterSenderError);
                this.counterSender.Dispose();
                this.counterSender = null;
            }
            if (this.socketSender != null)
            {
                this.socketSender.Dispose();
                this.socketSender = null;
            }
        }

        // Properties
        public static CounterPublisher DefaultInstance
        {
            get
            {
                if (defaultInstance == null)
                {
                    lock (syncRoot)
                    {
                        if (defaultInstance == null)
                        {
                            defaultInstance = new CounterPublisher(PhotonSettings.Default.CounterPublisher);
                        }
                    }
                }
                return defaultInstance;
            }
        }

        public EndPoint EndPoint
        {
            get
            {
                return this.endPoint;
            }
        }

        public ProtocolType Protocol
        {
            get
            {
                return this.protocol;
            }
        }

        // Nested Types
        public enum ProtocolType
        {
            Udp,
            Http,
            Pgm
        }
    }

}
