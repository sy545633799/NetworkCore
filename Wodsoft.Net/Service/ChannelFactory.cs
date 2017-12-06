using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Wodsoft.Net.Service
{
    public class ChannelFactory<T>
    {
        private Type Contract;
        internal T Instance;
        private ServiceClient Client;
        private ServiceChannel Channel;

        public ChannelFactory(ServiceClient client, ServiceChannel channel)
        {
            if (client == null)
                throw new ArgumentNullException("client");
            if (channel == null)
                throw new ArgumentNullException("channel");
            if (channel.Provider.Mode != ServiceMode.Client)
                throw new NotSupportedException("服务类型不是客户端类型。");
            Contract = typeof(T);
            if (Contract != channel.Provider.Contract)
                throw new ArgumentException("T类型与服务契约类型不一样。");
            Client = client;
            Channel = channel;            
        }

        public bool Exist
        {
            get
            {
                return Client.InvokeChannelExist(Channel.Name.ToLower().Trim());
            }
        }

        public T GetChannel()
        {
            if (Instance != null)
                return Instance;

            if (!Client.Connected)
                throw new InvalidOperationException("未连接至服务。");
            if (!Exist)
                throw new InvalidOperationException("服务频道不存在。");
            Instance = (T)Client.CreateInstance(Channel);
            return Instance;
        }
    }
}
