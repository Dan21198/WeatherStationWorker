using System.ComponentModel.DataAnnotations;

namespace WeatherStationWorker;

public class WeatherSettings
{
    [Required(ErrorMessage = "URL adresa meteostanice chybí v konfiguraci.")]
    [Url(ErrorMessage = "Zadaná hodnota není platná URL adresa.")]
    public string Url { get; set; } = string.Empty;
}