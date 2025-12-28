using System.IdentityModel.Tokens.Jwt;

using ABCD.Application;
using ABCD.Domain;
using ABCD.Infra.Data;
using ABCD.Lib;
using ABCD.Server;
using ABCD.Server.Middlewares;

using FluentValidation;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// Add configuration sources
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName.ToLowerInvariant()}.json", optional: true, reloadOnChange: true)
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
builder.Services.Configure<CachingSettings>(builder.Configuration.GetSection(CachingSettings.SectionName));

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

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<UserRegistrationValidator>();

builder.Services.Configure<WeatherForecastOptions>(builder.Configuration.GetSection("WeatherForecast"));
builder.Services.AddScoped<ICryptoService>(provider => {
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new CryptoService(configuration["Crypto:PassPhrase"]);
});

var passwordPolicy = builder.Configuration.GetSection("PasswordPolicy").Get<PasswordPolicy>();
builder.Services.AddScoped<PasswordPolicy>(provider => passwordPolicy);
builder.Services.AddScoped<IValidator<UserRegistration>, UserRegistrationValidator>();
builder.Services.AddScoped<IValidator<SignInCredentials>, SignInCredentialsValidator>();
builder.Services.AddScoped<SecurityTokenHandler, JwtSecurityTokenHandler>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<BearerTokenReader>();

builder.Services.AddScoped<RequestContextAccessor>();
builder.Services.AddScoped<RequestContext>(ctx => ctx.GetRequiredService<RequestContextAccessor>().RequestContext);
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<IBlogService, BlogService>();

var mapper = AutoMapperConfig.Initialize();
builder.Services.AddSingleton(mapper);
builder.Services.AddSingleton<IClassMapper, ClassMapper>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseMiddleware<TokenValidationMiddleware>();
app.UseAuthorization();
app.UseMiddleware<RequestContextMiddleware>();
app.MapControllers();

// Apply migrations
//MigrationHelper.ApplyMigrations(app.Services);

app.Run();