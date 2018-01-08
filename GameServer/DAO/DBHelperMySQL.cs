using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.DAO
{
    public class DBHelperMySQL
    {
        public void Add<T>()
        {
            using (var context = new DataContext())
            {
                //context.Blogs.Add(blog);
                //context.SaveChanges();
            }
        }
    }
}
