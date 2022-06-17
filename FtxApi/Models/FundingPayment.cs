using System;

namespace FtxApi.Models
{
    public class FundingPayment
    {
        public string Future { get; set; }
        public long Id { get; set; }
        public decimal Payment { get; set; }
        public DateTimeOffset Time { get; set; }
    }
}
