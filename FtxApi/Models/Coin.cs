namespace FtxApi.Models
{
    public class Coin
    {
        public bool CanDeposit { get; set; }
        public bool CanWithdraw { get; set; }
        public bool HasTag { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
