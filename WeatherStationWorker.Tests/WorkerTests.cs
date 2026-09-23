using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using WeatherStationWorker.Db;

namespace WeatherStationWorker.Tests;

public class WorkerTests
{
    [Fact]
    public async Task ProcessWeatherDataAsync_ValidXml_SavesJsonAndSetsIsOnlineTrue()
    {
        var dbName = Guid.NewGuid().ToString();
        
        var services = new ServiceCollection();
        services.AddDbContext<WeatherDbContext>(options => 
            options.UseInMemoryDatabase(databaseName: dbName));
            
        var serviceProvider = services.BuildServiceProvider();
        
        var options = Options.Create(new WeatherSettings { Url = "https://test.com/data.xml" });
        
        var handlerMock = new Mock<HttpMessageHandler>();
        const string fakeXml = "<Weather><Temperature>22.5</Temperature></Weather>";
        
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(fakeXml)
            });

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerMock.Object));
        
        var worker = new Worker(new Mock<ILogger<Worker>>().Object, serviceProvider, options, factoryMock.Object);
        
        await worker.ProcessWeatherDataAsync(CancellationToken.None);

        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
        var records = await db.WeatherRecords.ToListAsync();
        
        Assert.Single(records);
        Assert.True(records[0].IsOnline);
        Assert.Null(records[0].ErrorMessage); 
        Assert.Contains("22.5", records[0].JsonData!);
    }

    [Fact]
    public async Task ProcessWeatherDataAsync_ApiIsDown_SavesErrorAndSetsIsOnlineFalse()
    {
        var dbName = Guid.NewGuid().ToString();
        
        var services = new ServiceCollection();
        services.AddDbContext<WeatherDbContext>(options => 
            options.UseInMemoryDatabase(databaseName: dbName));
            
        var serviceProvider = services.BuildServiceProvider();

        var options = Options.Create(new WeatherSettings { Url = "https://test.com/data.xml" });
        
        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), 
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.InternalServerError 
            });

        var factoryMock = new Mock<IHttpClientFactory>();
        factoryMock.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient(handlerMock.Object));

        var worker = new Worker(new Mock<ILogger<Worker>>().Object, serviceProvider, options, factoryMock.Object);


        await worker.ProcessWeatherDataAsync(CancellationToken.None);
        
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<WeatherDbContext>();
        var records = await db.WeatherRecords.ToListAsync();
        
        Assert.Single(records);
        Assert.False(records[0].IsOnline);
        Assert.NotNull(records[0].ErrorMessage);
        Assert.Null(records[0].JsonData);
    }
}