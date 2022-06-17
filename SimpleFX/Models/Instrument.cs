namespace SimpleFX.Models
{
    public class Quote
    {
        public decimal? A { get; set; }
        public decimal? B { get; set; }
        public string? S { get; set; }
        public int T { get; set; }
    }
    public class Session
    {
        public int Open { get; set; }
        public int Close { get; set; }
    }
    public class Instrument
    {
        public int From { get; set; }
        public int To { get; set; }
        public string? Symbol { get; set; }
        public string? DisplayName { get; set; }
        public string? Tags { get; set; }
        public Quote? Quote { get; set; }
        public string? Securitie { get; set; }
        public string? Type { get; set; }
        public string? PriceCurrency { get; set; }
        public string? MarginCurrency { get; set; }
        public string? Description { get; set; }
        public int MarginPercentage { get; set; }
        public int ContractSize { get; set; }
        public int Digits { get; set; }
        public int MarginMode { get; set; }
        public int StopsLevel { get; set; }
        public double Step { get; set; }
        public int MaxSize { get; set; }
        public decimal SwapLong { get; set; }
        public decimal SwapShort { get; set; }
        public int Rollover3Days { get; set; }
        public int TradeMode { get; set; }
        public bool LongOnly { get; set; }
        public List<Session>? Sessions { get; set; }
        public string? CountryCode { get; set; }
        public string? FlagName { get; set; }
        public decimal TargetSpread { get; set; }
        public string? Isin { get; set; }
        public DateTime ExpiryDate { get; set; }
        public object? Info { get; set; }
        public string? StockExchangeName { get; set; }
        public string? LinkToExchange { get; set; }
        public string? Ticker { get; set; }
        public object? TradingHours { get; set; }
        public int ExecutionMode { get; set; }
        public decimal MarginDivider { get; set; }
        public int StepCents { get; set; }
        public int ProfitMode { get; set; }
        public int MaxSizeCents { get; set; }
        public int SwapType { get; set; }
        public decimal OnePip { get; set; }
    }
}
