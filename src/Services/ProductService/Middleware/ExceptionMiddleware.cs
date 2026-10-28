
using System.Net;
using System.Text.Json;


namespace ProductService.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger; // 1. Inject ILogger

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var traceId = context.TraceIdentifier; // Or your custom Trace ID format

                // 2. CRITICAL: Log the exception to stdout using Serilog / ILogger with the TraceId!
                _logger.LogError(ex, "An unhandled exception occurred during request processing. TraceId: {TraceId}", traceId);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var response = new
                {
                    statusCode = context.Response.StatusCode,
                    message = ex.Message,
                    details = ex.StackTrace,
                    traceId = traceId
                };

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}