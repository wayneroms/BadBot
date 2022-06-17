using System;

namespace FtxApi.Models.LeveragedTokens
{
    public class LeveragedTokenCreation
    {
        public long Id { get; set; }
        public string Token { get; set; }
        public decimal RequestedSize { get; set; }
        public bool Pending { get; set; }
        public decimal CreatedSize { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
        public decimal Fee { get; set; }
        public DateTimeOffset RequestedAt { get; set; }
        public DateTimeOffset FulfilledAt { get; set; }
    }
}
