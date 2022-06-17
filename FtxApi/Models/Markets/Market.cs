namespace FtxApi.Models.Markets
{
    public class Market
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Underlying { get; set; }
        public string BaseCurreny { get; set; }
        public string QuoteCurrency { get; set; }
        public bool Enabled { get; set; }
        public decimal? Ask { get; set; }
        public decimal? Bid { get; set; }
        public decimal? Last { get; set; }
        public decimal? PriceIncrement { get; set; }
        public decimal? SizeIncrement { get; set; }
    }
}
