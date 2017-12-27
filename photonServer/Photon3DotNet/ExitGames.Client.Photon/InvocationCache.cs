namespace ExitGames.Client.Photon
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    internal class InvocationCache
    {
        private readonly LinkedList<CachedOperation> cache = new LinkedList<CachedOperation>();

        private int nextInvocationId = 1;

        public bool inCache(int invocationId)
        {
            lock (this.cache)
            {
                LinkedListNode<CachedOperation> node = this.cache.First;
                while (node != null)
                {
                    if (node.Value.InvocationId == invocationId)
                    {
                        return true;
                    }
                    if (node.Value.InvocationId > invocationId)
                    {
                        break;
                    }
                }
            }
            return false;
        }

        public void Invoke(int invocationId, Action action)
        {
            lock (this.cache)
            {
                if (invocationId < this.nextInvocationId)
                {
                    Debug.Print("InvocationId {0} - ignored", invocationId);
                }
                else if (invocationId == this.nextInvocationId)
                {
                    this.nextInvocationId++;
                    action();
                    if (this.cache.Count > 0)
                    {
                        LinkedListNode<CachedOperation> n = this.cache.First;
                        while ((n != null) && (n.Value.InvocationId == this.nextInvocationId))
                        {
                            this.nextInvocationId++;
                            n.Value.Action();
                            n = n.Next;
                            this.cache.RemoveFirst();
                        }
                    }
                }
                else
                {
                    CachedOperation op = new CachedOperation()
                    {
                        InvocationId = invocationId,
                        Action = action
                    };
                    if (this.cache.Count == 0)
                    {
                        this.cache.AddLast(op);
                    }
                    else
                    {
                        for (LinkedListNode<CachedOperation> node = this.cache.First; node != null; node = node.Next)
                        {
                            if (node.Value.InvocationId > invocationId)
                            {
                                this.cache.AddBefore(node, op);
                                break;
                            }
                        }
                        this.cache.AddLast(op);
                    }
                }
            }
        }

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

        private class CachedOperation
        {
            public Action Action { get; set; }
            public int InvocationId { get; set; }
        }
    }
}
