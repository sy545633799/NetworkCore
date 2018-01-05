using MySql.Data;
using MySql.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MySql
{
    class Start
    {
        public void Add <T>() 
        {
            using (var context = new DataContext())
            {
                //context.Blogs.Add(blog);
                //context.SaveChanges();
            }
        }
    }
}
