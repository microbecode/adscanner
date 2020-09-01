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

namespace AdScanner
{
    public class ScannerService
    {
        private readonly ScannerContext _db;

        public const string baseUrl = @"https://www.ss.lv";
        public const string searchUrl = @"/lv/real-estate/homes-summer-residences/cesis-and-reg/";

        public ScannerService(ScannerContext db)
        {
            _db = db;
        }

        public void PerformScan()
        {
            var data = PerformFullScan().ToList();
            _db.Ads.AddRange(data);
            _db.SaveChanges();
        }

        public IEnumerable<Ad> PerformFullScan()
        {
            var web = new HtmlWeb();

            var ads = new List<Ad>();
            foreach (var ad in PerformFrontScan())
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
                    } catch { }

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

  
        public IEnumerable<Ad> PerformFrontScan()
        {
            var web = new HtmlWeb();
            var doc = web.Load(baseUrl + searchUrl);
            //var doc = web.Load(@"c:\temp\myfile.html");

            var nodes = doc.DocumentNode.SelectNodes("//a[@class='am']");
            var i = 0;
            foreach (var node in nodes)
            {
                //i++;
                //if (i > 5)
                //{
                //    continue;
                //}
                var idNode = node.Attributes.FirstOrDefault(a => a.Name.ToLower() == "id");
                if (idNode == null)
                {
                    continue;
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

                
                

                yield return model;
            }
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
