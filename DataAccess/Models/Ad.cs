using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccess.Models
{
    public class Ad
    {
        public int Id { get; set; }
        public string SiteId { get; set; }
        public string SiteUrl { get; set; }
        public string City { get; set; }
        public string Region { get; set; }
        public string Village { get; set; }
        public string Address { get; set; }
        public decimal? Size { get; set; }

        public string SizeStr { get; set; }
        public int? Floors { get; set; }
        public int? Rooms { get; set; }
        public decimal? Area { get; set; }
        public string AreaStr { get; set; }
        public string Commodities { get; set; }
        public string Description { get; set; }
        public string PriceStr { get; set; }
        public decimal? Price { get; set; }
        public PriceTypeE? PriceType { get; set; }
        public DateTime? FirstSeen { get; set; }
        public DateTime? NotSeenAnymore { get; set; }
    }

    public enum PriceTypeE
    {
        Simple = 0,
        MonthlyRent = 1,
        Exchange
    }
}
