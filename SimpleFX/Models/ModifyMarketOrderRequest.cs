namespace SimpleFX.Models
{
    public class ModifyMarketOrderRequest
    {
        public int Login { get; set; }
        public string? Reality { get; set; }
        public long Id { get; set; }
        public decimal TakeProfit { get; set; }
        public decimal StopLoss { get; set; }
    }
}
