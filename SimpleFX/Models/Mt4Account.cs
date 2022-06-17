namespace SimpleFX.Models
{
    public class Mt4Account : AccountStatus
    {
        public string? Reality { get; set; }
        public int Login { get; set; }
        public string? Currency { get; set; }
        public int Leverage { get; set; }
        public int Bonus { get; set; }
    }
}
