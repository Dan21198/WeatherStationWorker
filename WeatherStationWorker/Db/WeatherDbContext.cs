using Microsoft.EntityFrameworkCore;
using WeatherStationWorker.Entities;

namespace WeatherStationWorker.Db;

public class WeatherDbContext(DbContextOptions<WeatherDbContext> options) : DbContext(options)
{
    public DbSet<WeatherDataRecord> WeatherRecords { get; set; }
}