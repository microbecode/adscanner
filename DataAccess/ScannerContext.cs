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
        public DbSet<Ad> Ads { get; set; }
    }
}
