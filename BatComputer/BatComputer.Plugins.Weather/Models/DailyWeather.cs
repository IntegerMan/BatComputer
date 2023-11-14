using Newtonsoft.Json;

namespace BatComputer.Plugins.Weather.Models;

public class DailyWeather
{
    public List<string> Time { get; set; }
    [JsonProperty("weather_code")]
    public List<int> WeatherCode { get; set; }
    [JsonProperty("apparent_temperature_max")]
    public List<decimal> ApparentTemperatureMax { get; set; }
    [JsonProperty("apparent_temperature_min")]
    public List<decimal> ApparentTemperatureMin { get; set; }
    [JsonProperty("precipitation_sum")]
    public List<decimal> Precipitation { get; set; }
    [JsonProperty("rain_sum")]
    public List<decimal> Rain { get; set; }
    [JsonProperty("showers_sum")]
    public List<decimal> Showers { get; set; }
    [JsonProperty("snowfall_sum")]
    public List<decimal> Snowfall { get; set; }
    [JsonProperty("precipitation_probability_max")]
    public List<int> PrecipitationChanceMax { get; set; }
}