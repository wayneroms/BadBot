namespace FtxApi.Models.LeveragedTokens
{
    public class LeveragedToken
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Underlying { get; set; }
        public decimal Outstanding { get; set; }
        public decimal PricePerShare { get; set; }
        public decimal PositionPerShare { get; set; }
        public decimal UnderlyingMark { get; set; }
        public string ContractAddress { get; set; }
        public decimal Change1h { get; set; }
        public decimal Change24h { get; set; }
    }
}
