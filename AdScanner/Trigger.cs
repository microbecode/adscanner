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
        private readonly EmailSenderService _emailer;

        public Trigger(ScannerService service, EmailSenderService emailer)
        {
            _service = service;
            _emailer = emailer;
        }

        [FunctionName(nameof(Trigger))]
        public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            Random rnd = new Random();
            var chance = rnd.Next(1, 120);
            if (chance < 110)
            {
                log.LogInformation($"Attempted to run trigger but didn't get past the randomness: {chance} {DateTime.Now}");
                return;
            }

            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            try
            {
                await _service.PerformScan();
                log.LogInformation("Scan finished");
            }
            catch (Exception e)
            {
                log.LogError(e, "Unable to perform scan");
                await _emailer.SendError($"Exception in trigger: {e.Message}");
                throw e;
            }
        }
    }
}
