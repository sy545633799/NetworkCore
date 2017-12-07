using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wodsoft.Net.Service
{
    internal class ServiceChannelManager
    {
        private Dictionary<string, ServiceChannel> Channels;

        public ServiceChannelManager()
        {
            Channels = new Dictionary<string, ServiceChannel>();
        }

        public bool RegisterChannel(ServiceChannel channel)
        {
            if (Channels.ContainsKey(channel.Name.ToLower()))
                return false;
            Channels.Add(channel.Name.ToLower(), channel);
            return true;
        }

        public bool Exist(string channelName)
        {
            return Channels.ContainsKey(channelName.Trim().ToLower());
        }

        public ServiceChannel this[string channelName]
        {
            get
            {
                if (Channels.ContainsKey(channelName.Trim().ToLower()))
                    return Channels[channelName.Trim().ToLower()];
                return null;
            }
        }
    }
}
