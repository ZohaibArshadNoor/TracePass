using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using TracePass.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// 1. Service Registrations (BEFORE builder.Build())
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TracePass API",
        Version = "v1",
        Description = "Digital Product Passport & Compliance Platform API for Garment and Textile Exporters."
    });
});

// Register DbContext here
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Build the Application
var app = builder.Build();


// 3. HTTP Request Pipeline (AFTER builder.Build())
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "TracePass API v1");
        c.RoutePrefix = string.Empty;
    });
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

// Health Check Endpoint (Phase 0)
app.MapGet("/health", async (ApplicationDbContext dbContext) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync();

    return canConnect
        ? Results.Ok(new { status = "healthy", database = "connected" })
        : Results.Problem("Database connection failed");
});

app.MapControllers();

app.Run();