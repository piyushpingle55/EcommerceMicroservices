using AuthService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// 1. Add Entity Framework Core Database Context
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("EcommAuthDbConnection")));

// 2. Add Endpoints API Explorer & Swagger Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 3. Configure the HTTP request pipeline to enable Swagger in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AuthService API V1");
        c.RoutePrefix = string.Empty; // Makes Swagger UI load directly at the root URL (optional)
    });
}

app.UseAuthorization();
app.MapControllers();

app.Run();