namespace SimpleFX.Models
{
    public class GetOrderHistoryRequest
    {
        public int Login { get; set; }
        public string? Reality { get; set; }
        public long TimeFrom { get; set; }
        public long TimeTo { get; set; }
    }
}
