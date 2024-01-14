
using EchoServer.Echo;
using EchoServer.Probe;
using Serilog;


// Setup the logger (serilog)
var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
    .Build();

builder.Host.UseSerilog((hostContext, services, configuration) =>
{
    configuration.ReadFrom.Configuration(hostContext.Configuration);
});

// Add services to the container.
builder.Services.AddScoped<IEchoService, EchoService>();
builder.Services.AddScoped<IProbeService, ProbeService>();
builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Do NOT use https
// app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
