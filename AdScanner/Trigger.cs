using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AdScanner
{
    public class Trigger
    {
        private readonly ScannerService _service;
        public Trigger(ScannerService service)
        {
            _service = service;
        }

        [FunctionName(nameof(Trigger))]
        public void Run([TimerTrigger("0 0 */2 * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            _service.PerformScan();
        }
    }
}
