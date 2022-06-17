namespace SimpleFX.Models
{
    public class MarketExecutionReport
    {
        public AccountStatus? AccountStatus { get; set; }
        public List<MarketAction> MarketOrders { get; set; } = new List<MarketAction>();
    }
}
