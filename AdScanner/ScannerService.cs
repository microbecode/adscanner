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

namespace AdScanner
{
    public class ScannerService
    {
        private readonly ScannerContext _db;
        private readonly EmailSenderService _sender;

        public const string baseUrl = @"https://www.ss.lv";
        public const string searchUrlPage1 = @"/lv/real-estate/homes-summer-residences/cesis-and-reg/";
        public const string searchUrlPageN = "page{0}.html";

        public ScannerService(ScannerContext db, EmailSenderService sender)
        {
            _db = db;
            _sender = sender;
        }

        public async Task PerformScan()
        {

            var data = PerformFullScan().ToList();
            await SendChanges(data);

            _db.Ads.AddRange(data);
            _db.SaveChanges();
        }

        private async Task SendChanges(List<Ad> data)
        {
            var textList = new List<string>();
            textList.Add("New properties found<br/>");
            foreach (var ad in data)
            {
                string template = "Size: {0}, Price: {1}, Description: {2} <a target='_blank' href='{3}'>Link</a>";
                var row = string.Format(template, ad.Size, ad.PriceStr, ad.Description, ad.SiteUrl);
                textList.Add(row);
            }
            await _sender.Send(string.Join("<br/><br/>", textList));
        }

        public List<Ad> PerformFullScan()
        {
            var web = new HtmlWeb();

            var ads = new List<Ad>();
            var frontPageAds = PerformFrontScan();

            MarkExpired(frontPageAds);

            foreach (var ad in frontPageAds)
            {
                var exists = _db.Ads.Any(a => a.SiteId == ad.SiteId && a.PriceStr == ad.PriceStr);
                if (exists)
                {
                    continue;
                }
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
                        var sizeMatch = new Regex("(\\d+)").Match(GetNextElem(mainElem, "Platība:"));

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
                        var areaMatch = new Regex("(\\d+)").Match(GetNextElem(mainElem, "Zemes platība:"));
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
            var ids = frontPageAds.Select(a => a.SiteId);

            var expired = _db.Ads.Where(a => a.NotSeenAnymore == null && !ids.Contains(a.SiteId));
            foreach (var ex in expired)
            {
                ex.NotSeenAnymore = DateTime.UtcNow;
            }
            _db.SaveChanges();
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

                //var doc = web.Load(@"c:\temp\myfile.html");

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
