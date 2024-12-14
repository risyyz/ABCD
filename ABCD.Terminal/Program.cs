using ABCD.Services.Crypto;
using ABCD.Terminal;

using Cocona;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var builder = CoconaApp.CreateBuilder();

// Add configuration to the builder
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// Read the passphrase from the configuration
var passphrase = builder.Configuration["CryptoService:Passphrase"] ?? throw new InvalidOperationException("Passphrase not found in configuration");

// Register the CryptoService with the passphrase
builder.Services.AddSingleton<ICryptoService>(provider => new CryptoService(passphrase));

var app = builder.Build();
app.AddCommands<CryptoCommands>();

app.Run();
