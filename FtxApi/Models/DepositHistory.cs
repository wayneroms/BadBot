using System;

namespace FtxApi.Models
{
    public class DepositHistory
    {
        public string Coin { get; set; }
        public int Confirmations { get; set; }
        public DateTimeOffset ConfirmedTime { get; set; }
        public decimal Fee { get; set; }
        public long Id { get; set; }
        public DateTimeOffset SentTime { get; set; }
        public decimal Size { get; set; }
        public string Status { get; set; }
        public DateTimeOffset Time { get; set; }
        public string TxId { get; set; }
        public string Notes { get; set; }
    }
}
