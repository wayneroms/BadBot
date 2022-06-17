namespace FtxApi
{
    public class FtxConfig
    {
        public string ApiKey { get; set; }
        public string Secret { get; set; }

        public string Account
        {
            get => account;
            set => account = string.IsNullOrEmpty(value) ? null : value;
        }
        private string account = null;
        public bool HasAccount => Account != null;
    }
}
