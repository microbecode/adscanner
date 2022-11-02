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
using Azure.Data.Tables;

namespace AdScanner.Scanners
{
    public class HouseScanner : ScannerService
    {        
        private readonly ILogger<HouseScanner> _log;
        public const string searchUrlPage1 = @"/lv/real-estate/homes-summer-residences/cesis-and-reg/";        

        public HouseScanner(TableClient db, /* EmailSenderService sender, */ ILogger<HouseScanner> log)
        {
            _db = db;
            db.CreateIfNotExists();
            
            // https://lauriadscannerfnstor.table.core.windows.net/Houses
            //_sender = sender;
            _log = log;
        }

        //https://stackoverflow.com/a/55609365
        public string ToAzureKeyString(string str)
    {
        var sb = new StringBuilder();
        foreach (var c in str
            .Where(c => c != '/'
                        && c != '\\'
                        && c != '#'
                        && c != '/'
                        && c != '?'
                        && !char.IsControl(c)))
            sb.Append(c);
        return sb.ToString();
    }

        public string PerformScan()
        {
             var data = PerformFullScan().ToList();
             _log.LogInformation("Finished full house scan retrieval with {0} houses", data.Count);
             System.Console.WriteLine("Finished full house scan retrieval with {0} houses", data.Count);

             if (data.Count > 0)
             {
                var changes = GetChanges(data);
                foreach (var d in data) {
                    d.PartitionKey = d.Region;
                    d.RowKey = ToAzureKeyString(d.SiteId) + ToAzureKeyString(d.PriceStr);
                    System.Console.WriteLine(   "Adding entry with partition " + d.PartitionKey + " and rowkey " + d.RowKey);
                    _db.AddEntity(d);
                }
                _log.LogInformation("generated " + changes.Count() + " changes");
                return changes;
         }
             else
             {
                 _log.LogInformation("No new entries found");
                 System.Console.WriteLine("No new");
             }
            return null;
        }

        private string GetChanges(List<HouseData> data)
        {
            _log.LogInformation("Starting to create changes");
            var textList = new List<string>();
            textList.Add("New houses found<br/><ul>");
            foreach (var ad in data)
            {
                string template = "<li>Size: {0}, Price: {1}, Description: {2} <a target='_blank' href='{3}'>Link</a></li>";
                var row = string.Format(template, ad.SizeStr, ad.PriceStr, ad.Description, ad.SiteUrl);
                textList.Add(row);
            }

            return string.Join("", textList) + "</ul>";
        }

         public List<HouseData> PerformFullScan()
         {
             var web = new HtmlWeb();

             var ads = new List<HouseData>();
             var basics = PerformFrontScan(searchUrlPage1);

             _log.LogInformation("Found {0} front page ads", basics.Count);
             System.Console.WriteLine("Found {0} front page ads", basics.Count);

             //

             var allDbEntries = _db.Query<HouseData>(); //_db.Ads.AsNoTracking().ToList();
            System.Console.WriteLine("Found " + allDbEntries.Count() + " entries");

            MarkExpired(basics, allDbEntries);

             foreach (var basic in basics)
             {
                 var exists = allDbEntries.Any(a => a.SiteId == ToAzureKeyString(basic.SiteId) && a.PriceStr == ToAzureKeyString(basic.PriceStr));
                 if (exists)
                 {
                     continue;
                 }
                 _log.LogInformation($"Found new house: {basic.SiteId} and {basic.PriceStr}");      
                 System.Console.WriteLine($"Found new house: {basic.SiteId} and {basic.PriceStr}");         

                 var doc = web.Load(basic.SiteUrl);

                var mainElem = doc.DocumentNode.SelectSingleNode("//div[@id='content_sys_div_msg']");
                if (mainElem != null)
                {
                    var ad = new HouseData()
                    {
                        FirstSeen = DateTime.UtcNow,
                        SiteId = basic.SiteId,
                        SiteUrl = basic.SiteUrl,
                        PriceStr = basic.PriceStr
                    };

                    ad.Description = mainElem.SelectNodes("following::text()").First().InnerText;
                    ad.City = GetNextElem(mainElem, "Pilsēta, rajons:");
                    ad.Region = GetNextElem(mainElem, "Pilsēta/pagasts:");
                    ad.Village = GetNextElem(mainElem, "Ciems:");
                    ad.Address = GetNextElem(mainElem, "Iela:");

                    try
                    {
                        var sizeStr = GetNextElem(mainElem, "Platība:");
                        ad.SizeStr = sizeStr;
                        var sizeMatch = new Regex("(\\d+)").Match(sizeStr);

                        if (sizeMatch.Success && decimal.TryParse(sizeMatch.Captures[0].Value, out var sizeDec))
                        {
                            ad.Size = sizeDec;
                        }
                    }
                    catch { }

                    if (int.TryParse(GetNextElem(mainElem, "Stāvu skaits:"), out var floorsInt))
                    {
                        ad.Floors = floorsInt;
                    }
                    if (int.TryParse(GetNextElem(mainElem, "Istabas:"), out var roomsInt))
                    {
                        ad.Rooms = roomsInt;
                    }

                    try
                    {
                        var areaStr = GetNextElem(mainElem, "Zemes platība:");
                        ad.AreaStr = areaStr;
                        var areaMatch = new Regex("(\\d+)").Match(areaStr);
                        if (areaMatch.Success && decimal.TryParse(areaMatch.Captures[0].Value, out var areaDec))
                        {
                            ad.Area = areaDec;
                        }
                    }
                    catch { }

                    ad.Commodities = GetNextElem(mainElem, "Ērtības:");

                    ads.Add(ad);
                }
             }
             return ads;
         }

        internal void MarkExpired(List<BasicModel> basics, Azure.Pageable<HouseData> allDbEntries)
        {
            var ids = basics.Select(a => a.SiteId).Distinct().ToList();

            var expired = allDbEntries.Where(a => a.NotSeenAnymore == null && !ids.Contains(a.SiteId)).ToList();
            foreach (var ex in expired)
            {
                var dbEntry = _db.Query<HouseData>(ent => ent.RowKey == ex.RowKey).First();
                dbEntry.NotSeenAnymore = DateTime.UtcNow;
                _db.UpdateEntity(dbEntry, Azure.ETag.All, TableUpdateMode.Replace);
            }
        }
     }
}

