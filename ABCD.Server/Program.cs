using System.IdentityModel.Tokens.Jwt;

using ABCD.Application;
using ABCD.Domain;
using ABCD.Infra.Data;
using ABCD.Lib;
using ABCD.Server;
using ABCD.Server.Middlewares;
using ABCD.Server.Models;

using FluentValidation;

using Mapster;

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
    options.ConnectionString = configuration.GetConnectionString("DefaultConnection")!;
    options.CryptoPassPhrase = configuration["Crypto:PassPhrase"]!;
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
    options.TokenValidationParameters = jwtSettings!.GetTokenValidationParameters();
    
    // Configure to read JWT token from cookie instead of Authorization header
    options.Events = new JwtBearerEvents {
        OnMessageReceived = context => {
            // Read token from cookie
            context.Token = context.Request.Cookies[AppConstants.ACCESS_TOKEN];
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();

// Add CORS configuration to allow credentials (for cookie-based auth)
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAngularClient", policy => {
        policy.WithOrigins("https://localhost:4200", "http://localhost:4200")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Required for cookies
    });
});

builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserCommandValidator>();

builder.Services.Configure<WeatherForecastOptions>(builder.Configuration.GetSection("WeatherForecast"));
builder.Services.AddScoped<ICryptoService>(provider => {
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new CryptoService(configuration["Crypto:PassPhrase"]!);
});

var passwordPolicy = builder.Configuration.GetSection("PasswordPolicy").Get<PasswordPolicy>();
builder.Services.AddScoped<PasswordPolicy>(provider => passwordPolicy!);
builder.Services.AddScoped<IValidator<RegisterUserCommand>, RegisterUserCommandValidator>();
builder.Services.AddScoped<IValidator<SignInCommand>, SignInCommandValidator>();
builder.Services.AddScoped<SecurityTokenHandler, JwtSecurityTokenHandler>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<BearerTokenReader>();

builder.Services.AddScoped<RequestContextAccessor>();
builder.Services.AddScoped<RequestContext>(ctx => ctx.GetRequiredService<RequestContextAccessor>().RequestContext);
builder.Services.AddScoped<IBlogRepository, BlogRepository>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IPostService, PostService>();


var config = TypeAdapterConfig.GlobalSettings;

// Example: Map domain Post to PostResponseModel
config.NewConfig<Post, PostSummaryResponse>()
    .Map(dest => dest.Status, src => src.Status.ToString())
    .Map(dest => dest.PathSegment, src => src.PathSegment != null ? src.PathSegment : null )
    .Map(dest => dest.PostId, src => src.PostId != null ? src.PostId.Value : 0)
    .Map(dest => dest.BlogId, src => src.BlogId.Value)
    .Map(dest => dest.Version, src => src.Version != null ? src.Version.HexString : null);

config.NewConfig<Fragment, FragmentResponse>()
    .Map(dest => dest.FragmentId, src => src.FragmentId != null ? src.FragmentId.Value : 0)
    .Map(dest => dest.FragmentType, src => src.FragmentType.ToString())
    .Map(dest => dest.Content, src => src.Content != null ? src.Content : null)
    .Map(dest => dest.Position, src => src.Position);

config.NewConfig<Post, PostDetailResponse>()
    .Map(dest => dest.PostId, src => src.PostId != null ? src.PostId.Value : 0)
    .Map(dest => dest.BlogId, src => src.BlogId.Value)
    .Map(dest => dest.Status, src => src.Status.ToString())
    .Map(dest => dest.PathSegment, src => src.PathSegment != null ? src.PathSegment.Value : null)
    .Map(dest => dest.Version, src => src.Version != null ? src.Version.HexString : null)
    .Map(dest => dest.Fragments, src => src.Fragments.Adapt<List<FragmentResponse>>());

// Register the config and mapper
builder.Services.AddSingleton(config);
builder.Services.AddSingleton<ITypeMapper, TypeMapper>();

var app = builder.Build();

if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Configure the HTTP request pipeline.
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseHttpsRedirection();
app.UseCors("AllowAngularClient"); // Enable CORS for cookie-based auth
app.UseAuthentication();
app.UseMiddleware<TokenValidationMiddleware>();
app.UseAuthorization();
app.UseMiddleware<RequestContextMiddleware>(); //TODO: do not use this middleware for live blog - only for backend scenarios
app.MapControllers();

// Apply migrations
//MigrationHelper.ApplyMigrations(app.Services);

app.Run();