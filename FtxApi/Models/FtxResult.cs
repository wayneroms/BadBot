namespace FtxApi.Models
{
    public class FtxResult<T>
    {
        public bool Success { get; set; }
        public string Error { get; set; }
        public T Result { get; set; }
    }
}
