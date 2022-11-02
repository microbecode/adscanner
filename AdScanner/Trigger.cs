using System;
using System.Threading.Tasks;
using AdScanner.Scanners;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AdScanner
{
    public class Trigger
    {
        private readonly HouseScanner _houseService;
        //private readonly LandScanner _landService;
        private readonly EmailSenderService _emailer;

        public Trigger(HouseScanner houseService , EmailSenderService emailer )
        {
            _houseService = houseService;
         //   _landService = landService;
            _emailer = emailer;
        }

        [FunctionName(nameof(Trigger))]
        public async Task Run([TimerTrigger("0 */5 * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            System.Console.WriteLine(   "Triggered");
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
                var housesChange = _houseService.PerformScan();
                log.LogInformation("House scan finished");
                System.Console.WriteLine("House scan finished");

                // var landsChange = _landService.PerformScan();
                // log.LogInformation("Land scan finished");

                if (housesChange != null)
                {
                    await _emailer.Send(housesChange);
                }
            }
            catch (Exception e)
            {
                log.LogError(e, "Unable to perform scan");
                //await _emailer.SendError($"Exception in trigger: {e.Message}");
                throw;
            }
        }
    }
}
