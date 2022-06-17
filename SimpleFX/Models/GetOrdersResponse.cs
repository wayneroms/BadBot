namespace SimpleFX.Models
{
    public class GetOrdersResponse
    {
        public List<MarketOrder> MarketOrders { get; set; } = new List<MarketOrder>();
        public List<PendingOrder> PendingOrders { get; set; } = new List<PendingOrder>();
    }
}
