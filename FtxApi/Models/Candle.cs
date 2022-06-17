using System;

namespace FtxApi.Models
{
    public class Candle
    {
        public decimal Close { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Open { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public decimal Volume { get; set; }
    }
}
