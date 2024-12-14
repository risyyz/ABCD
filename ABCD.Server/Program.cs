using ABCD.Data;
using ABCD.Lib;
using ABCD.Server;

using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Add configuration sources
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment())
    builder.Configuration.AddUserSecrets<Program>();

// Bind Settings class to configuration
var configuration = builder.Configuration;
builder.Services.Configure<Settings>(options => {
    options.ConnectionString = configuration.GetConnectionString("DefaultConnection");
    options.CryptoPassPhrase = configuration["Crypto:PassPhrase"];
});

// Add services to the container.
builder.Services.AddAuthorization();
builder.Services.Configure<WeatherForecastOptions>(builder.Configuration.GetSection("WeatherForecast"));
builder.Services.AddDbContext<DataContext>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();

app.UseAuthorization();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (HttpContext httpContext, IOptions<WeatherForecastOptions> options, IOptions<Settings> settings, DataContext dbContext) => {
    var summaries = options.Value.Summaries;
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55)
        })
        .ToArray();
    return forecast;
});

app.MapFallbackToFile("/index.html");

// Apply migrations
MigrationHelper.ApplyMigrations(app.Services);

app.Run();
