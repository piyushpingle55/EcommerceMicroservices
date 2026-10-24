using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Middleware;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting ProductService web application...");

    var builder = WebApplication.CreateBuilder(args);


    #region  Configure Serilog to write to both Console and Loki
    builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("service_name", "ProductService")
    .WriteTo.Console()
    .WriteTo.GrafanaLoki(
        "http://localhost:3100",
        labels: new[]
        {
            new Serilog.Sinks.Grafana.Loki.LokiLabel { Key = "service_name", Value = "ProductService" },
            new Serilog.Sinks.Grafana.Loki.LokiLabel { Key = "job", Value = "ProductService" }
        },
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information));
    #endregion



    // Add services to the container.
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    #region Register Database Context for SQL Server
    builder.Services.AddDbContext<ProductDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("ProductDatabase")));
    #endregion

    #region Enable CORS for Angular frontend (running on localhost:4200)
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAngular", policy =>
        {
            policy.WithOrigins("http://localhost:4200")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
    #endregion

    var app = builder.Build();

    #region  Register custom global exception middleware , 1 in pipeline to catch.
    app.UseMiddleware<ExceptionMiddleware>();
    #endregion

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseHttpsRedirection();

    app.UseCors("AllowAngular");

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex )
{
    Log.Fatal(ex, "ProductService terminated unexpectedly during startup.");
}
finally
{
    Log.CloseAndFlush();
}