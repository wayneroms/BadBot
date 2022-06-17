using System;

namespace FtxApi.Models
{
    public class TriggerOrder
    {
        public DateTimeOffset CreatedAt { get; set; }
        public string Future { get; set; }
        public long Id { get; set; }
        public string Market { get; set; }
        public decimal TriggerPrice { get; set; }
        public long? OrderId { get; set; }
        public string Side { get; set; }
        public decimal Size { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
        public decimal? OrderPrice { get; set; }
        public string Error { get; set; }
        public DateTimeOffset? TriggeredAt { get; set; }
        public bool ReduceOnly { get; set; }
        public decimal? TrailStart { get; set; }
        public decimal? TrailValue { get; set; }
        public DateTimeOffset? CancelledAt { get; set; }
        public string CancelReason { get; set; }
        public string OrderStatus { get; set; }
        public string OrderType { get; set; }
        public bool RetryUntilFilled { get; set; }
        public decimal? FilledSize { get; set; }
        public decimal? AvgFillPrice { get; set; }
    }
}
