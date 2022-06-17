namespace FtxApi
{
    public static class SideType
    {
        public const string Buy = "buy";
        public const string Sell = "sell";

        public static string Reverse(string side)
            => side == Buy
            ? Sell
            : Buy;
    }
}
