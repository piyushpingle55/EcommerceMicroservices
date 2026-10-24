using ProductService.Models;
using Serilog.Context;
using System.Net;
using System.Text.Json;

namespace ProductService.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Push TraceId into Serilog LogContext for all logs generated during this request
            using (LogContext.PushProperty("TraceId", context.TraceIdentifier))
            {
                try
                {
                    await _next(context);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An unhandled exception occurred during request execution: {Message}", ex.Message);

                    context.Response.ContentType = "application/json";
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                    var response = _env.IsDevelopment()
                        ? new ErrorDetails
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = ex.Message,
                            Details = ex.StackTrace,
                            TraceId = context.TraceIdentifier
                        }
                        : new ErrorDetails
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = "An internal server error occurred.",
                            TraceId = context.TraceIdentifier
                        };

                    var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                    var json = JsonSerializer.Serialize(response, options);

                    await context.Response.WriteAsync(json);
                }
            }
        }
    }
}
