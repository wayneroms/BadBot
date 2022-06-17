using System.Collections.Generic;

namespace FtxApi.Models
{
    public class AccountInfo
    {
        public bool BackstopProvider { get; set; }
        public decimal Collateral { get; set; }
        public decimal FreeCollateral { get; set; }
        public decimal InitialMarginRequirement { get; set; }
        public bool Liquidating { get; set; }
        public decimal MaintenanceMarginRequirement { get; set; }
        public decimal MakerFee { get; set; }
        public decimal? MarginFraction { get; set; }
        public decimal? OpenMarginFraction { get; set; }
        public decimal TakerFee { get; set; }
        public decimal TotalAccountValue { get; set; }
        public decimal TotalPositionSize { get; set; }
        public string Username { get; set; }
        public decimal Leverage { get; set; }
        public List<Position> Positions { get; set; }
    }
}
