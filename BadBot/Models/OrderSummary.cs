using System.Text.Json.Serialization;

namespace BadBot
{
    public class OrderSummary
    {
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.Now;
        public string Market { get; set; } = string.Empty;
        public string Side { get; set; } = string.Empty;
        public decimal Size { get; set; }
        public decimal EntryPrice { get; set; }
        public long EntryId { get; set; }
        public decimal ProfitPrice { get; set; }
        public decimal StopPrice { get; set; }
        public long ProfitId { get; set; }
        public long StopId { get; set; }

        public string? Account
        {
            get => account;
            set => account = string.IsNullOrEmpty(value) ? null : value;
        }
        private string? account = null;

        [JsonIgnore]
        public bool HasAccount => Account != null;

    }
}
