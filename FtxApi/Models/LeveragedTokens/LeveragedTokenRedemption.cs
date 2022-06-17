using System;

namespace FtxApi.Models.LeveragedTokens
{
    public class LeveragedTokenRedemption
    {
        public long Id { get; set; }
        public string Token { get; set; }
        public decimal Size { get; set; }
        public decimal ProjectedProceeds { get; set; }
        public bool Pending { get; set; }
        public DateTimeOffset RequestedAt { get; set; }
    }
}
