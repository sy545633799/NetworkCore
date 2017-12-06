using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Wodsoft.Net.Service
{
    internal class MessageIOManager
    {
        private Dictionary<Guid, AutoResetEvent> Messages;
        private Dictionary<Guid, byte[]> IOs;
        private MessageIORecycle Recycle;

        public MessageIOManager()
        {
            Messages = new Dictionary<Guid, AutoResetEvent>();
            IOs = new Dictionary<Guid, byte[]>();
            Timeout = 30000;
            Recycle = new MessageIORecycle();
        }

        public void BeginMessage(Guid messageID, int timeout)
        {
            AutoResetEvent autoReset = new AutoResetEvent(false);
            Monitor.Enter(Messages);
            Messages.Add(messageID, autoReset);
            Monitor.Exit(Messages);

            MessageIO io = new MessageIO();
            io.ID = messageID;
            io.AutoReset = autoReset;
            io.Timeout = false;

            var data = Recycle.GetData(messageID);
            if (data == null)
            {

                ThreadPool.RegisterWaitForSingleObject(autoReset, RemoveMessage, io, timeout, true);

                autoReset.WaitOne();
            }
            else
            {
                IOs.Add(messageID, data);
            }

            Monitor.Enter(Messages);
            Messages.Remove(messageID);
            Monitor.Exit(Messages);
            if (io.Timeout)
                throw new TimeoutException("连接超时。");
        }

        public void BeginMessage(Guid messageID)
        {
            BeginMessage(messageID, Timeout);
        }

        public void SetMessage(Guid messageID, byte[] result)
        {
            if (!Messages.ContainsKey(messageID))
            {
                Recycle.Push(messageID, result);
                return;
            }
            var autoReset = Messages[messageID];
            IOs.Add(messageID, result);
            autoReset.Set();
        }

        public byte[] EndMessage(Guid messageID)
        {
            if (!IOs.ContainsKey(messageID))
                return null;
            var data = IOs[messageID];
            IOs.Remove(messageID);
            return data;
        }

        public int Timeout { get; set; }

        private void RemoveMessage(object state, bool timedOut)
        {
            MessageIO io = (MessageIO)state;
            io.Timeout = timedOut;
            io.AutoReset.Set();
        }
    }

    internal class MessageIO
    {
        public Guid ID { get; set; }
        public AutoResetEvent AutoReset { get; set; }
        public bool Timeout { get; set; }
    }
}
