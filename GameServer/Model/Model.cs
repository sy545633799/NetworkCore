using GameServer.DAO;
using GameServer.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.Model
{
    public abstract class Model<T> where T: Model<T>
    { 
        protected void Add(Action<DataContext> callback)
        {
            using (var context = new DataContext())
            {
                context.Database.EnsureCreated();
                callback(context);
                context.SaveChanges();
            }
        }

        public abstract void Add();
    }
}
