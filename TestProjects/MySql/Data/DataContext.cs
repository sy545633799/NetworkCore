using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MySql.Models;

namespace MySql.Data
{
    public class DataContext: DbContext
    {
        public DbSet<User> Users { set; get; }

        public DbSet<Blog> Blogs { set; get; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseMySQL(@"Server=localhost;database=test01;uid=root;pwd=3.1415926;SslMode=None");
        
    }
}
