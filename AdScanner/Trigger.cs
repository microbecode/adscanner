using System;
using System.Threading.Tasks;
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
        public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            Random rnd = new Random();
            if (rnd.Next(1, 200) < 192)
            {
                return;
            }

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            await _service.PerformScan();
        }
    }
}
