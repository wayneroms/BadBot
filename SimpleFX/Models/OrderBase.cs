namespace SimpleFX.Models
{
    public abstract class OrderBase
    {
        public long Id { get; set; }
        public string? Symbol { get; set; }
        public string? Reality { get; set; }
        public int Login { get; set; }
        public string? Side { get; set; }
        public decimal Volume { get; set; }
        public decimal TakeProfit { get; set; }
        public decimal StopLoss { get; set; }
        public string? Comment { get; set; }
        public string? CloseTime { get; set; }
    }
}
