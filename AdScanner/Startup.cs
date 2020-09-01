﻿using AdScanner;
using DataAccess;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

[assembly: FunctionsStartup(typeof(Startup))]
namespace AdScanner
{


    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = config["SqlConnectionString"];

            builder.Services.AddDbContext<ScannerContext>(
                options => SqlServerDbContextOptionsExtensions.UseSqlServer(options, connectionString));

            builder.Services.AddTransient<ScannerService>();
            builder.Services.AddSingleton<Trigger>();
        }
    }
}