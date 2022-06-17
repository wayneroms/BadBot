namespace SimpleFX.Models
{
    public class MarketOrder : OrderBase
    {
        public long OpenTime { get; set; }
        public decimal OpenPrice { get; set; }
        public decimal Margin { get; set; }
        public decimal Profit { get; set; }
        public decimal Swaps { get; set; }
        public decimal OpenConversionRate { get; set; }
        public decimal CloseConversionRate { get; set; }
    }
}
