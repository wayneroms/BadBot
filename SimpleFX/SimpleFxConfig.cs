namespace SimpleFX
{
    public class SimpleFxConfig
    {
        public string Id { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;

        public string? Reality
        {
            get => reality;
            set => reality = string.IsNullOrEmpty(value) ? null : value;
        }
        private string? reality = null;
        public bool HasReality => Reality != null;

        public string? Account
        {
            get => account;
            set => account = string.IsNullOrEmpty(value) ? null : value;
        }
        private string? account = null;
        public bool HasAccount => Account != null;
        public int AccountNo => int.Parse(account ?? "0");

    }
}
