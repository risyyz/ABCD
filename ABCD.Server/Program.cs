using System.IdentityModel.Tokens.Jwt;
using System.Text;

using ABCD.Data;
using ABCD.Lib;
using ABCD.Lib.Auth;
using ABCD.Server;
using ABCD.Server.Middlewares;
using ABCD.Services;

using FluentValidation;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add configuration sources
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

if (builder.Environment.IsDevelopment()) {
    builder.Configuration.AddUserSecrets<Program>();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options => {
        options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme {
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });

        options.OperationFilter<SecurityRequirementsOperationFilter>();
    }); 
}

// Bind Settings class to configuration
var configuration = builder.Configuration;
builder.Services.Configure<Settings>(options => {
    options.ConnectionString = configuration.GetConnectionString("DefaultConnection");
    options.CryptoPassPhrase = configuration["Crypto:PassPhrase"];
});

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

// Add services to the container.
var connectionString = configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDbContext<AuthContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
     .AddEntityFrameworkStores<AuthContext>()
     .AddDefaultTokenProviders();

builder.Services.AddMemoryCache();
builder.Services.AddSingleton<JsonWebTokenHandler>();

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options => {
    options.TokenValidationParameters = jwtSettings.GetTokenValidationParameters();
});


builder.Services.AddAuthorization();
//builder.Services.AddIdentityApiEndpoints<ApplicationUser>()
//    .AddEntityFrameworkStores<AuthContext>();

builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<UserRegistrationValidator>();

builder.Services.Configure<WeatherForecastOptions>(builder.Configuration.GetSection("WeatherForecast"));
builder.Services.AddScoped<ICryptoService>(provider => {
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new CryptoService(configuration["Crypto:PassPhrase"]);
});

builder.Services.AddScoped<IValidator<UserRegistration>, UserRegistrationValidator>();
builder.Services.AddScoped<IValidator<SignInCredentials>, SignInCredentialsValidator>();
builder.Services.AddScoped<ISecurityTokenValidator, JwtSecurityTokenHandler>();
builder.Services.AddScoped<IAuthService, AuthService>();

var mapper = AutoMapperConfig.Initialize();
builder.Services.AddSingleton(mapper);

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.MapIdentityApi<ApplicationUser>();

// Configure the HTTP request pipeline.
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<TokenValidationMiddleware>();
app.UseAuthorization();
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (HttpContext httpContext, IOptions<WeatherForecastOptions> options) => {
    var summaries = options.Value.Summaries;
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55)
        })
        .ToArray();
    return forecast;
}).RequireAuthorization();

app.MapControllers();

// Apply migrations
MigrationHelper.ApplyMigrations(app.Services);

app.Run();