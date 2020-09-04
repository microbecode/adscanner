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

namespace AdScanner
{
    public class ScannerService
    {
        private readonly ScannerContext _db;
        private readonly EmailSenderService _sender;
        private readonly ILogger<Trigger> _log;

        public const string baseUrl = @"https://www.ss.lv";
        public const string searchUrlPage1 = @"/lv/real-estate/homes-summer-residences/cesis-and-reg/";
        public const string searchUrlPageN = "page{0}.html";

        public ScannerService(ScannerContext db, EmailSenderService sender, ILogger<Trigger> log)
        {
            _db = db;
            _sender = sender;
            _log = log;
        }

        public async Task PerformScan()
        {
            var data = PerformFullScan().ToList();
            _log.LogInformation("Finished full scan retrieval with {0} entries", data.Count);
            if (data.Count > 0)
            {
                _log.LogInformation($"Sending email with {data.Count} entries");
                await SendChanges(data);

                _db.Ads.AddRange(data);
                _db.SaveChanges();
            }
            else
            {
                _log.LogInformation("No new entries found");
            }
        }

        private async Task SendChanges(List<Ad> data)
        {
            var textList = new List<string>();
            textList.Add("New properties found<br/><ul>");
            foreach (var ad in data)
            {
                string template = "<li>Size: {0}, Price: {1}, Description: {2} <a target='_blank' href='{3}'>Link</a></li>";
                var row = string.Format(template, ad.SizeStr, ad.PriceStr, ad.Description, ad.SiteUrl);
                textList.Add(row);
            }

            await _sender.Send(string.Join("", textList) + "</ul>");            
        }

        public List<Ad> PerformFullScan()
        {
            var web = new HtmlWeb();

            var ads = new List<Ad>();
            var frontPageAds = PerformFrontScan();

            _log.LogInformation("Found {0} front page ads", frontPageAds.Count);

            MarkExpired(frontPageAds);
            _log.LogInformation("Passed expiration");

            var allAds = _db.Ads.AsNoTracking().ToList();

            //_log.LogInformation("Retrieved all ads, {0}", allAds.Count);

            foreach (var ad in frontPageAds)
            {
                var exists = allAds.Any(a => a.SiteId == ad.SiteId && a.PriceStr == ad.PriceStr);
                if (exists)
                {
                    //_log.LogInformation("Exists, continue {0}", ad.SiteId);
                    continue;
                }
                _log.LogInformation($"Found new data: {ad.SiteId}");
                ad.FirstSeen = DateTime.UtcNow;

                var doc = web.Load(ad.SiteUrl);

                var mainElem = doc.DocumentNode.SelectSingleNode("//div[@id='content_sys_div_msg']");
                if (mainElem != null)
                {
                    ad.Description = mainElem.SelectNodes("following::text()").First().InnerText;
                    ad.City = GetNextElem(mainElem, "Pilsēta/pagasts:");
                    ad.Region = GetNextElem(mainElem, "Pilsēta, rajons:");
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

                    //try
                    //{
                    //    var priceStr = mainElem.SelectNodes("//td[@class='ads_price']").First().InnerText;
                    //    ad.PriceStr = priceStr;
                    //} catch { }

                    ads.Add(ad);
                }
            }
            return ads;
        }

        private void MarkExpired(List<Ad> frontPageAds)
        {
            var ids = frontPageAds.Select(a => a.SiteId).ToList();

            var expired = _db.Ads.Where(a => a.NotSeenAnymore == null && !ids.Contains(a.SiteId)).ToList();
            foreach (var ex in expired)
            {
                ex.NotSeenAnymore = DateTime.UtcNow;
            }
            if (expired.Count > 0)
            {
                _db.SaveChanges();
            }
        }

        public List<Ad> PerformFrontScan()
        {
            var ads = new List<Ad>();

            var web = new HtmlWeb();

            var page1 = baseUrl + searchUrlPage1;
            var pages = new List<string>()
            {
                page1
            };
            for (int i = 2; i < 20; i++)
            {
                var newPage = page1 + string.Format(searchUrlPageN, i);
                pages.Add(newPage);
            }

            foreach (var page in pages)
            {
                var doc = web.Load(page);

                var nodes = doc.DocumentNode.SelectNodes("//a[@class='am']");
                var i = 0;
                foreach (var node in nodes)
                {
                    var idNode = node.Attributes.FirstOrDefault(a => a.Name.ToLower() == "id");
                    if (idNode == null)
                    {
                        continue;
                    }
                    if (ads.Count > 0 && ads[0].SiteId == idNode.Value)
                    {
                        // Non-existing page brings us to front page, no point in checking anymore
                        return ads;
                    }

                    var anchor = node.Attributes.FirstOrDefault(a => a.Name.ToLower() == "href");
                    if (anchor == null)
                    {
                        continue;
                    }

                    var price = GetNextElemTwoUp(node);

                    var fullUrl = baseUrl + anchor.Value;
                    var model = new Ad()
                    {
                        SiteId = idNode.Value,
                        SiteUrl = fullUrl,
                        PriceStr = price
                    };

                    ads.Add(model);
                }
            }
            return ads;
        }

        private string GetNextElem(HtmlNode root, string content)
        {
            try
            {
                var text = root.SelectNodes($"//td[text() = '{content}']").First().ParentNode.SelectNodes("td[last()]").First().InnerText;
                return text;
            }
            catch
            {
                return null;
            }
        }

        private string GetNextElemTwoUp(HtmlNode root)
        {
            try
            {
                var text = root.ParentNode.ParentNode.ParentNode.SelectNodes("td[last()]").First().InnerText;
                return text;
            }
            catch
            {
                return null;
            }
        }

    }
}
