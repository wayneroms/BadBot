namespace SimpleFX.Models
{
    public class PendingExecutionReport
    {
        public AccountStatus? AccountStatus { get; set; }
        public List<PendingAction> PendingOrders { get; set; } = new List<PendingAction>();
    }
}
