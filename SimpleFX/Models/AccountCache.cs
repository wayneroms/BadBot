namespace SimpleFX.Models
{
    internal class AccountCache
    {
        public int Account { get; set; }
        public string? Reality { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public Mt4Account Mt4Account { get; set; }
    }
}
