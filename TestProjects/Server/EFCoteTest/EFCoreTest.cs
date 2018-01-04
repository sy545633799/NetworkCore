using MySql.Data;
using MySql.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.EFCoteTest
{
    public class EFCoreTest
    {
        public void Test()
        {
            using (var context = new DataContext())
            {
                context.Database.EnsureCreated();

                //context.Add(new User { Name = "愤怒的TryCatch" });
                //User user = new User { Name = "小鸟的愤怒" };

                //context.Add(user);
                //context.Remove<User>(user);
                //User user = new User { Name = "新的小鸟5"};

                User usr = new User { Name = "test01", Age = 18 };
                context.Add(usr);
                context.SaveChanges();

                //User user = context.Find<User>(6);
                //Console.WriteLine(user.Name);
                //Console.ReadKey();

            }
        }
    }
}
