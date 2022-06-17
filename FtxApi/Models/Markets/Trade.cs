using System;

namespace FtxApi.Models.Markets
{
    public class Trade
    {
        public decimal Id { get; set; }
        public bool Liquidation { get; set; }
        public decimal Price { get; set; }
        public string Side { get; set; }
        public decimal Size { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}
