using System;

namespace FtxApi.Models.LeveragedTokens
{
    public class LeveragedTokenRedemptionRequest
    {
        public long Id { get; set; }
        public string Token { get; set; }
        public decimal Size { get; set; }
        public bool Pending { get; set; }
        public decimal Price { get; set; }
        public decimal Proceeds { get; set; }
        public decimal Fee { get; set; }
        public DateTimeOffset RequestedAt { get; set; }
        public DateTimeOffset FulfilledAt { get; set; }
    }
}
