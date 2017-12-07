using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Wodsoft.Net.Service
{
    internal class MessageIORecycle
    {
        private int TotalCount { get; set; }
        private Queue<Guid> RecycleQueue;
        private Dictionary<Guid, byte[]> RecycleCategory;

        public MessageIORecycle()
        {
            RecycleQueue = new Queue<Guid>();
            RecycleCategory = new Dictionary<Guid, byte[]>();
        }

        public void Push(Guid id, byte[] data)
        {
            lock (this)
            {
                TotalCount += data.Length;
                while (TotalCount > 16777216)
                {
                    var delete = RecycleQueue.Dequeue();
                    var deleteData = RecycleCategory[delete];
                    TotalCount -= deleteData.Length;
                    RecycleCategory.Remove(delete);
                }
                RecycleQueue.Enqueue(id);
                RecycleCategory.Add(id, data);
            }
        }
        
        public byte[] GetData(Guid id)
        {
            lock (this)
            {
                if (!RecycleCategory.ContainsKey(id))
                    return null;
                var result = RecycleCategory[id];
                RecycleCategory.Remove(id);
                TotalCount -= result.Length;
                return result;
            }
        }
    }
}
