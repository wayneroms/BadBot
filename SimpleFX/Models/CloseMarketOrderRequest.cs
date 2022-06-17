namespace SimpleFX.Models
{
    internal class CloseMarketOrderRequest
    {
        public long Id { get; set; }
        public decimal Volume { get; set; }
        public string? RequestId { get; set; }
        public int Login { get; set; }
        public string? Reality { get; set; }
    }
}
