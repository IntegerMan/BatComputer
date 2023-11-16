using Newtonsoft.Json;

namespace MattEland.BatComputer.Plugins.Weather.Models;

public class CurrentWeather
{
    [JsonProperty("temperature_2m")]
    public decimal Temperature { get; set; }

    [JsonProperty("apparent_temperature")]
    public decimal ApparentTemperature { get; set; }

    public decimal Precipitation { get; set; }
    public decimal Rain { get; set; }
    public decimal Showers { get; set; }
    public decimal Snowfall { get; set; }
    [JsonProperty("weather_code")]
    public int WeatherCode { get; set; }
    [JsonProperty("cloud_cover")]
    public decimal CloudCoverPercent { get; set; }
    [JsonProperty("is_day")]
    public bool IsDay { get; set; }
}