using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wodsoft.Net.Service
{
    public class ServiceUnity
    {
        private Dictionary<Type, object> Instances;

        public ServiceUnity()
        {
            Instances = new Dictionary<Type, object>();
        }

        public void RegisterType<T>(object instance) where T : class
        {
            Type type = typeof(T);
            if (instance == null)
                throw new ArgumentNullException("instance");
            if (Instances.ContainsKey(type))
                Instances[type] = instance;
            else
                Instances.Add(type, instance);
        }

        public void UnregisterType<T>(object instance) where T : class
        {
            Type type = typeof(T);
            if (Instances.ContainsKey(type))
                Instances.Remove(type);
        }

        public object GetInstance(Type type)
        {
            if (Instances.ContainsKey(type))
                return Instances[type];
            return null;
        }

        public bool IsRegistered<T>()
        {
            return IsRegistered(typeof(T));
        }

        public bool IsRegistered(Type type)
        {
            return Instances.ContainsKey(type);
        }
    }
}
