using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkCore.IOCP
{
    public class UserTokenPool
    {
        private Stack<UserToken> m_pool;

        public UserTokenPool(int capacity)
        {
            m_pool = new Stack<UserToken>(capacity);
        }

        public void Push(UserToken item)
        {
            if (item == null)
            {
                throw new ArgumentException("Items added to a AsyncSocketUserToken cannot be null");
            }
            lock (m_pool)
            {
                m_pool.Push(item);
            }
        }

        public UserToken Pop()
        {
            lock (m_pool)
            {
                return m_pool.Pop();
            }
        }

        public int Count
        {
            get { return m_pool.Count; }
        }
    }
}
