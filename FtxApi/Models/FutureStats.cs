using System;

namespace FtxApi.Models
{
    public class FutureStats
    {
        public decimal Volume { get; set; }
        public decimal NextFundingRate { get; set; }
        public DateTimeOffset NextFundingTime { get; set; }
        public decimal ExpirationPrice { get; set; }
        public decimal PredictedExpirationPrice { get; set; }
        public decimal OpenInterest { get; set; }
        public decimal StrikePrice { get; set; }
    }
}
