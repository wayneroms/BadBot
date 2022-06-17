namespace SimpleFX.Models
{
    public class CloseMarketOrdersRequest
    {
        public string? Symbol { get; set; }
        public List<long> OrderIds { get; set; } = new List<long>();
        public string? RequestId { get; set; }
        public int Login { get; set; }
        public string? Reality { get; set; }
    }
}
