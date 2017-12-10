using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using test2.Models;

namespace test2.Data
{
    public class DataContext: DbContext
    {
        public DbSet<User> Users { set; get; }

        public DbSet<Blog> Blogs { set; get; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseMySQL(@"Server=localhost;database=ef;uid=root;pwd=3.1415926;SslMode=None");
    }
}
