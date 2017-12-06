using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wodsoft.Net.Service
{
    public class ServiceChannel
    {
        public ServiceChannel(string channelName, ServiceProvider provider)
        {
            if (string.IsNullOrWhiteSpace(channelName))
                throw new ArgumentNullException("channelName");
            if (provider == null)
                throw new ArgumentNullException("provider");
            Name = channelName.Trim();
            Provider = provider;
        }

        public ServiceChannel(string name, ServiceProvider provider, DataFormatter formatter)
            : this(name, provider)
        {
            Formatter = formatter;
        }

        public string Name { get; private set; }

        public ServiceProvider Provider { get; private set; }

        public DataFormatter Formatter { get; private set; }
    }
}
