using AdScanner;
using AdScanner.Scanners;
using Azure.Data.Tables;
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

            var sendGridApiKey = config["SendGridApiKey"];
            var basicReceiver = config["BasicEmailReceiver"];

            builder.Services.AddSingleton<TableClient>(options => new TableClient(config["TableConnectionString"], "Houses"));

            //builder.Services.AddDbContext<ScannerContext>(
            //    options => options.UseSqlServer(connectionString, providerOptions => providerOptions.EnableRetryOnFailure()));

            builder.Services.AddTransient<HouseScanner>();
            System.Console.WriteLine(  "Startup end");
           // builder.Services.AddTransient<LandScanner>();
            builder.Services.AddSingleton<EmailSenderService>(options => new EmailSenderService(sendGridApiKey, basicReceiver));
        }
    }
}
