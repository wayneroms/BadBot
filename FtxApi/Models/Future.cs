using System;

namespace FtxApi.Models
{
    public class Future
    {
        public decimal? Ask { get; set; }
        public decimal? Bid { get; set; }
        public decimal Change1h { get; set; }
        public decimal Change24h { get; set; }
        public decimal ChangeBod { get; set; }
        public decimal VolumeUsd24h { get; set; }
        public decimal Volume { get; set; }
        public string Description { get; set; }
        public bool Enabled { get; set; }
        public bool Expired { get; set; }
        public DateTimeOffset? Expiry { get; set; }
        public decimal Index { get; set; }
        public decimal ImfFactor { get; set; }
        public decimal? Last { get; set; }
        public decimal LowerBound { get; set; }
        public decimal Mark { get; set; }
        public string Name { get; set; }
        public bool Perpetual { get; set; }
        public decimal PositionLimitWeight { get; set; }
        public bool PostOnly { get; set; }
        public decimal PriceIncrement { get; set; }
        public decimal SizeIncrement { get; set; }
        public string Underlying { get; set; }
        public decimal UpperBound { get; set; }
        public string Type { get; set; }
    }
}