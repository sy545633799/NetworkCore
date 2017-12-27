using System.Configuration;
using System.Net;

namespace Photon.SocketServer.Diagnostics.Configuration
{
    public sealed class CounterPublisherSettings : ConfigurationElement
    {
        // Methods
        public bool TryGetIpEndpoint(out IPEndPoint endPoint)
        {
            IPAddress address;
            int num;
            endPoint = null;
            if (string.IsNullOrEmpty(this.Endpoint))
            {
                return false;
            }
            string[] strArray = this.Endpoint.Split(new char[] { ':' });
            if (strArray.Length != 2)
            {
                return false;
            }
            if (!IPAddress.TryParse(strArray[0], out address))
            {
                return false;
            }
            if (!int.TryParse(strArray[1], out num))
            {
                return false;
            }
            endPoint = new IPEndPoint(address, num);
            return true;
        }

        public bool TryGetSendInterface(out IPAddress address)
        {
            address = null;
            return (string.IsNullOrEmpty(this.SendInterface) || IPAddress.TryParse(this.SendInterface, out address));
        }

        // Properties
        [ConfigurationProperty("addDefaultAppCounter", IsRequired = false, DefaultValue = "True")]
        public bool AddDefaultAppCounter
        {
            get
            {
                return (bool)base["addDefaultAppCounter"];
            }
            set
            {
                base["addDefaultAppCounter"] = value;
            }
        }

        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = "False")]
        public bool Enabled
        {
            get
            {
                return (bool)base["enabled"];
            }
            set
            {
                base["enabled"] = value;
            }
        }

        [ConfigurationProperty("endpoint", IsRequired = true)]
        public string Endpoint
        {
            get
            {
                return (string)base["endpoint"];
            }
        }

        [ConfigurationProperty("maxCounterPerRequest", IsRequired = false, DefaultValue = "1000")]
        public int MaxCounterPerRequest
        {
            get
            {
                return (int)base["maxCounterPerRequest"];
            }
        }

        [ConfigurationProperty("maxQueueLength", IsRequired = false, DefaultValue = "120")]
        public int MaxQueueLength
        {
            get
            {
                return (int)base["maxQueueLength"];
            }
        }

        [ConfigurationProperty("protocol", IsRequired = false, DefaultValue = "udp")]
        public string Protocol
        {
            get
            {
                return (string)base["protocol"];
            }
            set
            {
                base["protocol"] = value;
            }
        }

        [ConfigurationProperty("publishInterval", IsRequired = false, DefaultValue = "10")]
        public int PublishInterval
        {
            get
            {
                return (int)base["publishInterval"];
            }
        }

        [ConfigurationProperty("senderId", IsRequired = false, DefaultValue = "")]
        public string SenderId
        {
            get
            {
                return (string)base["senderId"];
            }
        }

        [ConfigurationProperty("sendInterface", IsRequired = false)]
        public string SendInterface
        {
            get
            {
                return (string)base["sendInterface"];
            }
        }

        [ConfigurationProperty("updateInterval", IsRequired = false, DefaultValue = "1")]
        public int UpdateInterval
        {
            get
            {
                return (int)base["updateInterval"];
            }
        }
    }

}
