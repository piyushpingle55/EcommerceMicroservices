namespace ProductService.Models
{
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; } // Stack trace or inner details for development
        public string TraceId { get; set; } = string.Empty;
    }
}
