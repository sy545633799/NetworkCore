using System;
using System.Collections.Generic;

namespace Photon.SocketServer.Web
{
    internal class InvocationCache
    {
        // Fields
        private readonly LinkedList<CachedOperation> cache = new LinkedList<CachedOperation>();
        private int nextInvocationId;

        // Methods
        public void Invoke(int invocationId, Action action)
        {
            lock (this.cache)
            {
                if (invocationId == this.nextInvocationId)
                {
                    this.nextInvocationId++;
                    action();
                    if (this.cache.Count > 0)
                    {
                        LinkedListNode<CachedOperation> first = this.cache.First;
                        while ((first != null) && (first.Value.InvocationId == this.nextInvocationId))
                        {
                            this.nextInvocationId++;
                            first.Value.Action();
                            first = first.Next;
                            this.cache.RemoveFirst();
                        }
                    }
                }
                else
                {
                    CachedOperation operation = new CachedOperation
                    {
                        InvocationId = invocationId,
                        Action = action
                    };
                    if (this.cache.Count == 0)
                    {
                        this.cache.AddLast(operation);
                    }
                    else
                    {
                        for (LinkedListNode<CachedOperation> node2 = this.cache.First; node2 != null; node2 = node2.Next)
                        {
                            if (node2.Value.InvocationId > invocationId)
                            {
                                this.cache.AddBefore(node2, operation);
                                goto Label_010E;
                            }
                        }
                        this.cache.AddLast(operation);
                    Label_010E: ;
                    }
                }
            }
        }

        // Properties
        public int Count
        {
            get
            {
                return this.cache.Count;
            }
        }

        public int NextInvocationId
        {
            get
            {
                return this.nextInvocationId;
            }
        }

        // Nested Types
        private class CachedOperation
        {
            // Properties
            public Action Action { get; set; }

            public int InvocationId { get; set; }
        }
    }
}
