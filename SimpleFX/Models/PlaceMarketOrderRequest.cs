namespace SimpleFX.Models
{
    public class PlaceMarketOrderRequest
    {
        public string? Reality { get; set; }
        public int Login { get; set; }
        public string? Symbol { get; set; }
        public string? Side { get; set; }
        public decimal Volume { get; set; }
        public bool IsFIFO { get; set; }
        public decimal TakeProfit { get; set; }
        public decimal StopLoss { get; set; }
        public string? RequestId { get; set; }
    }
}
