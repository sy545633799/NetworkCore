using System.Configuration;

namespace Photon.SocketServer.Diagnostics.Configuration
{
    public sealed class PhotonSettings : ConfigurationSection
    {
        // Fields
        public static readonly PhotonSettings Default = ((ConfigurationManager.GetSection("Photon") as PhotonSettings) ?? new PhotonSettings());

        // Methods
        public PhotonSettings()
        {
            this.CounterPublisher = new CounterPublisherSettings();
        }

        // Properties
        [ConfigurationProperty("CounterPublisher", IsRequired = false)]
        public CounterPublisherSettings CounterPublisher
        {
            get
            {
                return (CounterPublisherSettings)base["CounterPublisher"];
            }
            private set
            {
                base["CounterPublisher"] = value;
            }
        }

        [ConfigurationProperty("MaxHttpReceiveMessageSize", IsRequired = false, DefaultValue = 0x100000)]
        public int MaxHttpReceiveMessageSize
        {
            get
            {
                return (int)base["MaxHttpReceiveMessageSize"];
            }
            set
            {
                base["MaxHttpReceiveMessageSize"] = value;
            }
        }
    }
}
