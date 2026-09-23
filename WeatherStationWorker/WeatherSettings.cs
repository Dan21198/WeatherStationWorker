using System.ComponentModel.DataAnnotations;

namespace WeatherStationWorker;

public class WeatherSettings
{
    [Required(ErrorMessage = "The weather station URL is missing in the configuration.")]
    [Url(ErrorMessage = "The entered value is not a valid URL.")]
    public string Url { get; set; } = string.Empty;
}
