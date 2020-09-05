using DataAccess.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess
{
    public class ScannerContext : DbContext
    {
        public ScannerContext(DbContextOptions<ScannerContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ad>()
                .HasIndex(b => b.SiteId);
            modelBuilder.Entity<Ad>()
                .HasIndex(b => b.PriceStr);
            modelBuilder.Entity<Ad>()
                .HasIndex(b => b.NotSeenAnymore);

            modelBuilder.Entity<Land>()
                .HasIndex(b => b.SiteId);
            modelBuilder.Entity<Land>()
                .HasIndex(b => b.PriceStr);
            modelBuilder.Entity<Land>()
                .HasIndex(b => b.NotSeenAnymore);
        }
        public DbSet<Ad> Ads { get; set; }
        public DbSet<Land> Lands { get; set; }
    }
}
