using System;

namespace FtxApi.Models
{
    public class Order
    {
        public DateTimeOffset CreatedAt { get; set; }
        public decimal FilledSize { get; set; }
        public string Future { get; set; }
        public long Id { get; set; }
        public string Market { get; set; }
        public decimal? Price { get; set; }
        public bool? Liquidation { get; set; }
        public decimal? AvgFillPrice { get; set; }
        public decimal RemainingSize { get; set; }
        public string Side { get; set; }
        public decimal Size { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public bool ReduceOnly { get; set; }
        public bool Ioc { get; set; }
        public bool PostOnly { get; set; }
        public string ClientId { get; set; }

        public bool IsBuy => Side == SideType.Buy;
        public bool IsSell => Side == SideType.Sell;
        public bool IsClosed => Status == OrderStatus.Closed;
    }
}
