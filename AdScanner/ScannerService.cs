using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.IO;
using AdScanner.Models;

namespace AdScanner
{
    public class ScannerService
    {
        public const string baseUrl = @"https://www.ss.lv";
        public const string searchUrl = @"/lv/real-estate/homes-summer-residences/cesis-and-reg/";

        public void PerformScan()
        {
            var web = new HtmlWeb();
            var doc = web.Load(@"c:\temp\a.html");
            //foreach (var ad in PerformFrontScan())
            //{

            //    var doc = web.Load(ad.SiteUrl);
            //    doc.Save(@"c:\temp\a.txt");

            //}

            var mainElem = doc.DocumentNode.SelectSingleNode("//div[@id='content_sys_div_msg']");
            if (mainElem != null)
            {
                //var cityElem = mainElem.SelectSingleNode();
                Console.WriteLine();

                //var tables = mainElem.SelectNodes("following-sibling::table").ToList();
                var desc = mainElem.SelectNodes("following::text()").First().InnerText;
                var city2 = mainElem.SelectNodes("//td[text() = 'Pilsēta/pagasts:']").First().ParentNode.SelectNodes("td[last()]").First().InnerText;
                var city = GetNextElem(mainElem, "Pilsēta/pagasts:");

                var priceStr = mainElem.SelectNodes("//td[@class='ads_price']").First().InnerText;
                //tables[0];
                /*

        public string Description { get; set; }
        public decimal Price { get; set; }
        public int PriceType { get; set; }
                */
            }
        }

        private string GetNextElem(HtmlNode root, string content)
        {
            var text = root.SelectNodes($"//td[text() = '{content}']").First().ParentNode.SelectNodes("td[last()]").First().InnerText;
            return text;
        }

        public IEnumerable<Ad> PerformFrontScan()
        {
            var web = new HtmlWeb();
            var doc = web.Load(baseUrl + searchUrl);

            var nodes = doc.DocumentNode.SelectNodes("//a[@class='am']");
            var i = 0;
            foreach (var node in nodes)
            {
                i++;
                if (i > 1)
                {
                    continue;
                }
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
                var fullUrl = baseUrl + anchor.Value;
                var model = new Ad()
                {
                    SiteId = idNode.Value,
                    SiteUrl = fullUrl
                };

                yield return model;
            }
        }
    }
}
