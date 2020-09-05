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
    public abstract class ScannerService
    {
        internal ScannerContext _db;
        internal EmailSenderService _sender;

        public const string baseUrl = @"https://www.ss.lv";
        public const string searchUrlPageN = "page{0}.html";

        public List<BasicModel> PerformFrontScan(string searchUrl)
        {
            var models = new List<BasicModel>();

            var web = new HtmlWeb();

            var page1 = baseUrl + searchUrl;
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
                    if (models.Count > 0 && models[0].SiteId == idNode.Value)
                    {
                        // Non-existing page brings us to front page, no point in checking anymore
                        return models;
                    }

                    var anchor = node.Attributes.FirstOrDefault(a => a.Name.ToLower() == "href");
                    if (anchor == null)
                    {
                        continue;
                    }

                    var price = GetNextElemTwoUp(node);

                    var fullUrl = baseUrl + anchor.Value;
                    var model = new BasicModel()
                    {
                        SiteId = idNode.Value,
                        SiteUrl = fullUrl,
                        PriceStr = price
                    };

                    models.Add(model);
                }
            }
            return models;
        }

        internal string GetNextElem(HtmlNode root, string content)
        {
            try
            {
                var text = root.SelectNodes($"//td[text() = '{content}']")?.First().ParentNode?.SelectNodes("td[last()]")?.First().InnerText;
                return text;
            }
            catch
            {
                return null;
            }
        }

        internal string GetNextElemTwoUp(HtmlNode root)
        {
            try
            {
                var text = root.ParentNode?.ParentNode?.ParentNode?.SelectNodes("td[last()]")?.First().InnerText;
                return text;
            }
            catch
            {
                return null;
            }
        }

    }

    public class BasicModel
    {
        public string SiteId { get; set; }

        public string SiteUrl { get; set; }

        public string PriceStr { get; set; }
    }
}
