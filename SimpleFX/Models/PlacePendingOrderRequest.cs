namespace SimpleFX.Models
{
    public class PlacePendingOrderRequest
    {
        public decimal ActivationPrice { get; set; }
        public long ExpiryTime { get; set; }
        public string? Symbol { get; set; }
        public decimal Volume { get; set; }
        public decimal TakeProfit { get; set; }
        public decimal StopLoss { get; set; }
        public string? Side { get; set; }
        public string? RequestId { get; set; }
        public int Login { get; set; }
        public string? Reality { get; set; }
    }
}
