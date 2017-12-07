using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wodsoft.Net.Service
{
    public class ServiceSessionState
    {
        private Dictionary<string, object> Store;

        public ServiceSessionState()
        {
            Store = new Dictionary<string, object>();
        }

        public object this[string key]
        {
            get
            {
                if (Store.ContainsKey(key))
                    return Store[key];
                return null;
            }
            set
            {
                if (Store.ContainsKey(key))
                    if (value != null)
                        Store[key] = value;
                    else
                        Store.Remove(key);
                if (value != null)
                    Store.Add(key, value);
            }
        }

        public void Clear()
        {
            Store.Clear();
        }
    }
}
