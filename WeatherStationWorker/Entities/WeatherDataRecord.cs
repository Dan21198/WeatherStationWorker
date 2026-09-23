using System.ComponentModel.DataAnnotations;

namespace WeatherStationWorker.Entities;

public class WeatherDataRecord
{
    public int Id { get; init; }
    public DateTime DownloadedAt { get; init; }
    public bool IsOnline { get; set; }
    [MaxLength]
    public string? JsonData { get; set; }
    [MaxLength(500)]
    public string? ErrorMessage { get; set; }
}