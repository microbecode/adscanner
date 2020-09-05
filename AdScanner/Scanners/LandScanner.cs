using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;
using DataAccess.Models;
using DataAccess;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace AdScanner.Scanners
{
    public class LandScanner : ScannerService
    {
        private readonly ILogger<LandScanner> _log;
        public const string searchUrlPage1 = @"/lv/real-estate/plots-and-lands/cesis-and-reg/";

        public LandScanner(ScannerContext db, EmailSenderService sender, ILogger<LandScanner> log)
        {
            _db = db;
            _sender = sender;
            _log = log;
        }

        public string PerformScan()
        {
            var data = PerformFullScan().ToList();
            _log.LogInformation("Finished full land scan retrieval with {0} lands", data.Count);
            if (data.Count > 0)
            {
                var changes = GetChanges(data);

                _db.Lands.AddRange(data);
                _db.SaveChanges();
                return changes;
            }
            else
            {
                _log.LogInformation("No new entries found");
            }
            return null;
        }

        private string GetChanges(List<Land> data)
        {
            var textList = new List<string>();
            textList.Add("New lands found<br/><ul>");
            foreach (var ad in data)
            {
                string template = "<li>Size: {0}, Price: {1}, Description: {2} <a target='_blank' href='{3}'>Link</a></li>";
                var row = string.Format(template, ad.SizeStr, ad.PriceStr, ad.Description, ad.SiteUrl);
                textList.Add(row);
            }

            return string.Join("", textList) + "</ul>";
        }

        public List<Land> PerformFullScan()
        {
            var web = new HtmlWeb();

            var objs = new List<Land>();
            var basics = PerformFrontScan(searchUrlPage1);

            _log.LogInformation("Found {0} front page ads", basics.Count);

            MarkExpired(basics);
            _log.LogInformation("Passed expiration");

            var allDbEntries = _db.Lands.AsNoTracking().ToList();

            foreach (var basic in basics)
            {
                var exists = allDbEntries.Any(a => a.SiteId == basic.SiteId && a.PriceStr == basic.PriceStr);
                if (exists)
                {
                    continue;
                }
                _log.LogInformation($"Found new land: {basic.SiteId}");

                var doc = web.Load(basic.SiteUrl);

                var mainElem = doc.DocumentNode.SelectSingleNode("//div[@id='content_sys_div_msg']");
                if (mainElem != null)
                {
                    var obj = new Land()
                    {
                        FirstSeen = DateTime.UtcNow,
                        SiteId = basic.SiteId,
                        SiteUrl = basic.SiteUrl,
                        PriceStr = basic.PriceStr
                    };

                    obj.Description = mainElem.SelectNodes("following::text()").First().InnerText;
                    obj.City = GetNextElem(mainElem, "Pilsēta, rajons:");
                    obj.Region = GetNextElem(mainElem, "Pilsēta/pagasts:");
                    obj.Village = GetNextElem(mainElem, "Ciems:");
                    obj.Address = GetNextElem(mainElem, "Iela:");

                    try
                    {
                        var sizeStr = GetNextElem(mainElem, "Platība:");
                        obj.SizeStr = sizeStr;
                        var sizeMatch = new Regex("(\\d+)").Match(sizeStr);

                        if (sizeMatch.Success && decimal.TryParse(sizeMatch.Captures[0].Value, out var sizeDec))
                        {
                            obj.Size = sizeDec;
                        }
                    }
                    catch { }

                    obj.Usage = GetNextElem(mainElem, "Pielietojums:");
                    obj.LandNumber = GetNextElem(mainElem, "Kadastra numurs:");

                    objs.Add(obj);
                }
            }
            return objs;
        }

        internal void MarkExpired(List<BasicModel> basics)
        {
            var ids = basics.Select(a => a.SiteId).ToList();

            var expired = _db.Lands.Where(a => a.NotSeenAnymore == null && !ids.Contains(a.SiteId)).ToList();
            foreach (var ex in expired)
            {
                ex.NotSeenAnymore = DateTime.UtcNow;
            }
            if (expired.Count > 0)
            {
                _db.SaveChanges();
            }
        }
    }
}
