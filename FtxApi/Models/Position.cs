namespace FtxApi.Models
{
    public class Position
    {
        public decimal Cost { get; set; }
        public decimal? CumulativeBuySize { get; set; }
        public decimal? CumulativeSellSize { get; set; }
        public decimal? EntryPrice { get; set; }
        public decimal? EstimatedLiquidationPrice { get; set; }
        public string Future { get; set; }
        public decimal? InitialMarginRequirement { get; set; }
        public decimal LongOrderSize { get; set; }
        public decimal MaintenanceMarginRequirement { get; set; }
        public decimal NetSize { get; set; }
        public decimal OpenSize { get; set; }
        public decimal RealizedPnl { get; set; }
        public decimal? RecentAverageOpenPrice { get; set; }
        public decimal? RecentBreakEvenPrice { get; set; }
        public decimal? RecentPnl { get; set; }
        public decimal ShortOrderSize { get; set; }
        public string Side { get; set; }
        public decimal Size { get; set; }
        public decimal UnrealizedPnl { get; set; }
        public decimal CollateralUsed { get; set; }
    }
}
