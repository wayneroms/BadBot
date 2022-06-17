using System;

namespace FtxApi.Models
{
    public class Fill
    {
        public decimal Fee { get; set; }
        public string FeeCurrency { get; set; }
        public decimal FeeRate { get; set; }
        public string Future { get; set; }
        public long Id { get; set; }
        public string Liquidity { get; set; }
        public string Market { get; set; }
        public string BaseCurrency { get; set; }
        public string QuoteCurrency { get; set; }
        public decimal? OrderId { get; set; }
        public decimal? TradeId { get; set; }
        public decimal Price { get; set; }
        public string Side { get; set; }
        public decimal Size { get; set; }
        public DateTimeOffset Time { get; set; }
        public string Type { get; set; }
    }
}
