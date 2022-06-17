using System.Text.Json.Serialization;

namespace BadBot
{
    public class WebhookRequest
    {
        /// <summary>
        /// Default: "enter".
        /// </summary>
        public string Cmd
        {
            get => cmd;
            set => cmd = value.ToLower();
        }
        private string cmd = "enter";

        [JsonIgnore]
        public string Command => cmd;
        [JsonIgnore]
        public bool IsClean => cmd == "clean";
        [JsonIgnore]
        public bool IsEntry => cmd == "enter" || cmd == "entry";
        [JsonIgnore]
        public bool IsExit => cmd == "exit";

        /// <summary>
        /// Market
        /// </summary>
        public string Sym { get; set; } = string.Empty;

        [JsonIgnore]
        public string Symbol => Sym;
        [JsonIgnore]
        public string FtxMarket => Sym.ToUpper();

        /// <summary>
        /// Direction: "buy" OR "sell"
        /// Default: "buy".
        /// </summary>
        public string Side
        {
            get => side;
            set => side = value.ToLower();
        }
        private string side = "buy";

        [JsonIgnore]
        public bool IsLong => side == "buy" || side == "long";
        [JsonIgnore]
        public bool IsShort => side == "sell" || side == "short";
        [JsonIgnore]
        public string FtxSide => IsLong ? FtxApi.SideType.Buy : FtxApi.SideType.Sell;

        /// <summary>
        /// Entry order type: "float" or "limit" or "market".
        /// Default: "float".
        /// </summary>
        public string Type
        {
            get => type;
            set => type = value.ToLower();
        }
        private string type = "limit";

        [JsonIgnore]
        public bool IsFloat => type == "float";
        [JsonIgnore]
        public bool IsLimit => type == "limit";
        [JsonIgnore]
        public bool IsMarket => type == "market";

        /// <summary>
        /// Position size as percent of total collateral.
        /// FTX: This value is multiplied by account leverage.
        /// </summary>
        public decimal Pc { get; set; }

        [JsonIgnore]
        public decimal Position => Pc;

        /// <summary>
        /// Position size as absolute value in base currency
        /// </summary>
        public decimal Val { get; set; }

        [JsonIgnore]
        public decimal Value => Val;

        /// <summary>
        /// Take profit percent from current price.
        /// </summary>
        public decimal Tp { get; set; }

        [JsonIgnore]
        public decimal ProfitPercent => Tp;

        /// <summary>
        /// Take profit as absolute value in base currency.
        /// </summary>
        public decimal TpPrice { get; set; }

        [JsonIgnore]
        public decimal ProfitPrice => TpPrice;

        /// <summary>
        /// Stop loss percent from current price.
        /// </summary>
        public decimal Sl { get; set; }

        [JsonIgnore]
        public decimal StopPercent => Sl;

        /// <summary>
        /// Stop loss as absolute value in base currency.
        /// </summary>
        public decimal SlPrice { get; set; }

        [JsonIgnore]
        public decimal StopPrice => SlPrice;

        /// <summary>
        /// If true, when entry is received in opposite direction, then existing position is closed and all existing orders cancelled.
        /// Default: true.
        /// </summary>
        public bool Swing { get; set; } = true;

        /// <summary>
        /// Account
        /// </summary>
        public string? Acc
        {
            get => acc;
            set => acc = string.IsNullOrEmpty(value) ? null : value;
        }
        private string? acc;

        [JsonIgnore]
        public string? Account => acc;
        [JsonIgnore]
        public bool HasAccount => acc != null;

        /// <summary>
        /// Handshake
        /// </summary>
        public string? Tkn { get; set; }

        /// <summary>
        /// Percent above entry for buy or below entry for sell price has moved to cancel order and resubmit.
        /// Default: 0.05%
        /// </summary>
        public decimal FloatThreshold { get; set; } = 0.05m;

        /// <summary>
        /// Price increment to offset below Bid price when buying, or above Ask price when selling.
        /// Default: 1
        /// Example:
        /// Minimum price increment: $2
        /// Ask: $56501
        /// Bid: $56500
        /// Side: Buy
        /// Limit offset: 5
        /// Limit order entry price = $56500 - ($2 * 5) = $56490 
        /// </summary>
        public int LimitPriceIncrementOffset { get; set; } = 1;

        public bool IsValid => IsClean || IsExit || IsValidEntry;

        public bool IsValidEntry
            => (IsLong || IsShort)
            && (IsClean || IsEntry || IsExit)
            && (IsFloat || IsLimit || IsMarket)
            && LimitPriceIncrementOffset >= 0
            && FloatThreshold >= 0
            && (Pc > 0 || Val > 0)
            && (Sl > 0 || SlPrice > 0)
            && (Tp > 0 || TpPrice > 0)
            && !string.IsNullOrEmpty(Sym)
            && !string.IsNullOrEmpty(Tkn)
            && !string.IsNullOrEmpty(Type);
    }
}
