using System;

namespace FtxApi.Models
{
    public class FundingRate
    {
        public string Future { get; set; }
        public decimal Rate { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}
