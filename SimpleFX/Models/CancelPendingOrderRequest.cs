namespace SimpleFX.Models
{
    public class CancelPendingOrderRequest
    {
        public long Id { get; set; }
        public string? RequestId { get; set; }
        public int Login { get; set; }
        public string? Reality { get; set; }
    }
}
