namespace SimpleFX.Models
{
    public class PendingOrder : OrderBase
    {
        public decimal ActivationPrice { get; set; }
        public string? Direction { get; set; }
        public string? CreateTime { get; set; }
        public string? ExpiryTime { get; set; }
    }
}
