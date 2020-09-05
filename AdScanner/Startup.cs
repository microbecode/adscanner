using AdScanner;
using AdScanner.Scanners;
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
            var sendGridApiKey = config["SendGridApiKey"];
            var basicReceiver = config["BasicEmailReceiver"];

            builder.Services.AddDbContext<ScannerContext>(
                options => options.UseSqlServer(connectionString, providerOptions => providerOptions.EnableRetryOnFailure()));

            builder.Services.AddTransient<HouseScanner>();
            builder.Services.AddTransient<LandScanner>();
            builder.Services.AddSingleton<EmailSenderService>(options => new EmailSenderService(sendGridApiKey, basicReceiver));
        }
    }
}
