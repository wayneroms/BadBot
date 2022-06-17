namespace FtxApi.Models
{
    public class WithdrawalHistory
    {
        public string Coin { get; set; }
        public string Address { get; set; }
        public string Tag { get; set; }
        public decimal Fee { get; set; }
        public long Id { get; set; }
        public decimal Size { get; set; }
        public string Status { get; set; }
        public string Time { get; set; }
        public string TxId { get; set; }
    }
}
