using Microsoft.EntityFrameworkCore;
using WeatherStationWorker;
using WeatherStationWorker.Db;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddOptions<WeatherSettings>()
    .BindConfiguration("WeatherStation")
    .ValidateDataAnnotations()
    .ValidateOnStart();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDbContext<WeatherDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHttpClient("WeatherClient")
    .AddStandardResilienceHandler();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();