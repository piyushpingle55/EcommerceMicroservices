var builder = WebApplication.CreateBuilder(args);

// 1. Add CORS policy for Angular frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// 2. Add YARP reverse proxy
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// 3. MUST BE BEFORE MapReverseProxy
app.UseCors("AllowAngular");

// 4. Map YARP routes
app.MapReverseProxy();

app.Run();