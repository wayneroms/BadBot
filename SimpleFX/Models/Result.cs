namespace SimpleFX.Models
{
    public class Result<T>
    {
        public T? Data { get; set; }
        public int Code { get; set; }
        public string? Message { get; set; }
        public string? WebRequestId { get; set; }
    }
}
