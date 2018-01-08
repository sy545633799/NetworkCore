using GameServer.Model;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace GameServer.DAO
{
    public class DataContext : DbContext
    {

        public DbSet<User> Users { set; get; }

        public DbSet<Account> Accounts { set; get; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseMySQL(@"Server=localhost;database=LoL;uid=root;pwd=3.1415926;SslMode=None");//The host localhost does not support SSL connections.


        //protected override void OnModelCreating(ModelBuilder modelBuilder)
        //{
        //    base.OnModelCreating(modelBuilder);
        //    modelBuilder.Entity<User>().HasIndex(u => u.Id).IsUnique();
        //    modelBuilder.Entity<Account>().HasIndex(u => u.Id).IsUnique();
        //}
    }
}
