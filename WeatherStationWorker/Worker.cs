using System.Xml.Linq;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using WeatherStationWorker.Entities;
using WeatherStationWorker.Db;

namespace WeatherStationWorker;

public class Worker(
    ILogger<Worker> logger, 
    IServiceProvider serviceProvider, 
    IOptions<WeatherSettings> settings, 
    IHttpClientFactory httpClientFactory)
    : BackgroundService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("WeatherClient");

    private readonly string _weatherUrl = settings.Value.Url;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            if (logger.IsEnabled(LogLevel.Information))
            {
                logger.LogInformation("Starting weather station data download at: {time}", DateTimeOffset.Now);
            }
            
            await ProcessWeatherDataAsync(stoppingToken);
            
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }

    public async Task ProcessWeatherDataAsync(CancellationToken stoppingToken)
    {
        var record = new WeatherDataRecord
        {
            DownloadedAt = DateTime.UtcNow
        };

        try
        {
            var response = await _httpClient.GetAsync(_weatherUrl, stoppingToken);
            response.EnsureSuccessStatusCode();

            var xmlContent = await response.Content.ReadAsStringAsync(stoppingToken);
            
            var xNode = XElement.Parse(xmlContent);
            record.JsonData = JsonConvert.SerializeXNode(xNode, Formatting.None, true);
            record.IsOnline = true;
            
            logger.LogInformation("Data successfully downloaded and saved.");
        }
        catch (Exception ex)
        {
            logger.LogWarning("Weather station is unavailable or an error occurred: {msg}", ex.Message);
            record.IsOnline = false;
            record.ErrorMessage = ex.Message;
        }
        
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
        
        await db.Database.EnsureCreatedAsync(stoppingToken);
        
        db.WeatherRecords.Add(record);
        await db.SaveChangesAsync(stoppingToken);
    }
}