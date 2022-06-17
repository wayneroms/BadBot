namespace SimpleFX.Models
{
    public class ModifyPendingOrderRequest
    {
        public long Id { get; set; }
        public decimal TakeProfit { get; set; }
        public decimal StopLoss { get; set; }
        public decimal ActivationPrice { get; set; }
        public long ExpiryTime { get; set; }
        public decimal Volume { get; set; }
        public string? RequestId { get; set; }
        public int Login { get; set; }
        public string? Reality { get; set; }
    }
}
