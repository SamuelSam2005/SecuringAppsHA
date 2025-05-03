namespace SecureDocumentExchange.Web.Models
{
    public class AccessLog
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string AccessedBy { get; set; }
        public string IpAddress { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
