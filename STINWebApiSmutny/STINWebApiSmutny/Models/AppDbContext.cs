using System.Collections.Generic;
using System;
using Microsoft.EntityFrameworkCore;

namespace STINWebApiSmutny.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Favorit> Favorites { get; set; } = null!;
    }
}
