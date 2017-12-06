using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wodsoft.Net.Service
{
    internal class HostInstanceManager
    {
        private Dictionary<ServiceProvider, object> GlobaInstances;
        private Dictionary<ServiceUser, Dictionary<ServiceProvider, object>> UserInstances;
        private ServiceUnity Unity;
        private InstanceTypeBuilder Builder;

        public HostInstanceManager(ServiceUnity unity)
        {
            Unity = unity;
            GlobaInstances = new Dictionary<ServiceProvider, object>();
            UserInstances = new Dictionary<ServiceUser, Dictionary<ServiceProvider, object>>();
            Builder = new InstanceTypeBuilder(OperationMode.Server);
        }

        private object CreateInstance(ServiceChannel channel, InvokeMethodDelegate invokeMethod, Type type)
        {
            InstanceProxy proxy = new InstanceProxy(channel, channel.Provider.ClientOperations.ToList(), invokeMethod);

            if (Unity.IsRegistered(type))
                return Unity.GetInstance(type);
            foreach (var constructor in type.GetConstructors())
            {
                var parameters = constructor.GetParameters();
                object[] parameterArray = new object[parameters.Length];
                bool parameterSuccess = true;
                parameterArray[0] = proxy;
                for (int i = 1; i < parameters.Length; i++)
                {
                    object obj = Unity.GetInstance(parameters[i].ParameterType);
                    if (obj == null)
                    {
                        parameterSuccess = false;
                        break;
                    }
                    parameterArray[i] = obj;
                }
                if (!parameterSuccess)
                    continue;
                try
                {
                    return Activator.CreateInstance(type, parameterArray);
                }
                catch
                {

                }
            }
            return null;
        }

        public object GetInstance(ServiceChannel channel, InvokeMethodDelegate invokeMethod)
        {
            if (GlobaInstances.ContainsKey(channel.Provider))
                return GlobaInstances[channel.Provider];
            Type type;
            try
            {
                type = Builder.GetInstanceType(channel.Provider.Instance, channel.Provider.Contract);
            }
            catch
            {
                return null;
            }
            object instance = CreateInstance(channel, invokeMethod, type);
            if (instance == null)
                return null;
            GlobaInstances.Add(channel.Provider, instance);
            return instance;
        }

        public void AddUser(ServiceUser user)
        {
            UserInstances.Add(user, new Dictionary<ServiceProvider, object>());
        }

        public void RemoveUser(ServiceUser user)
        {
            UserInstances.Remove(user);
        }

        public object GetInstance(ServiceChannel channel, InvokeMethodDelegate invokeMethod, ServiceUser user)
        {
            if (channel.Provider.Mode == ServiceMode.Single)
                return GetInstance(channel, invokeMethod);
            if (UserInstances[user].ContainsKey(channel.Provider))
                return UserInstances[user][channel.Provider];
            Type type;
            try
            {
                type = Builder.GetInstanceType(channel.Provider.Instance, channel.Provider.Contract);
            }
            catch
            {
                return null;
            }
            object instance = CreateInstance(channel, invokeMethod, type);
            if (channel.Provider.Mode == ServiceMode.PreClient)
                UserInstances[user].Add(channel.Provider, instance);
            return instance;
        }
    }
}
