using System.Collections.Generic;

namespace FtxApi.Models.Markets
{
    public class Orderbook
    {
        public List<List<decimal>> Bids { get; set; }
        public List<List<decimal>> Asks { get; set; }
    }
}
