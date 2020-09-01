using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DataAccess
{
        public class ScannerContextFactory : IDesignTimeDbContextFactory<ScannerContext>
        {
            /// <summary>
            /// dotnet tooling is needing this
            /// </summary>
            /// <param name="args"></param>
            /// <returns></returns>
            public ScannerContext CreateDbContext(string[] args)
            {
                var builder = new ConfigurationBuilder()
                    .AddJsonFile(Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json"), optional: true);
                var configuration = builder.Build();

                var connectionString = configuration["SqlConnectionString"];

                var optionsBuilder = new DbContextOptionsBuilder<ScannerContext>()
                    .UseSqlServer(connectionString);
                return new ScannerContext(optionsBuilder.Options);
            }
        }


    
}
