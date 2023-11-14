namespace BatComputer.Plugins.Weather.Models;

public class WeatherResponse
{
    public CurrentWeather? Current { get; set; }
    public DailyWeather? Daily { get; set; }
}