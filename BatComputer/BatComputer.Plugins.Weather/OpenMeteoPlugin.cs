using System;
using System.ComponentModel;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace BatComputer.Plugins.Weather;


public class OpenMeteoPlugin
{

    [SKFunction, Description("Gets a latitude and longitude string from a city, zip code, or location. Result is formatted like: lat,long")]
    public async Task<string> GetLatLong([Description("The city or location such as Columbus, Ohio or a Zip code like 43081")] string location)
    {

        // If we include a comma, ignore the comma and content to the right of it
        if (location.Contains(','))
        {
            location = location[..location.IndexOf(',', StringComparison.Ordinal)];
        }

        string url = $"https://geocoding-api.open-meteo.com/v1/search?name={UrlEncoder.Default.Encode(location)}&count=1";

        string json = await GetJsonFromRestRequestAsync(url);
        GeoCodingResponse? geoCodingResponse = JsonConvert.DeserializeObject<GeoCodingResponse>(json);

        if (geoCodingResponse?.Results == null || geoCodingResponse.Results.Count == 0)
        {
            return "Could not interpret lat / long from JSON: " + json;
        }

        GeoCodingLocation geo = geoCodingResponse.Results.First();

        return geo.Latitude + "," + geo.Longitude;
    }

    private static async Task<string> GetJsonFromRestRequestAsync(string url)
    {
        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(url);
        string json = await response.Content.ReadAsStringAsync();
        return json;
    }

    [SKFunction, Description("Gets current weather information from a latitude and longitude")]
    public async Task<string> GetCurrentWeatherFromLatLong([Description("The latitude and longitude. Formatted like: lat,long")] string latLong)
    {
        if (!latLong.Contains(','))
        {
            return latLong + " does not appear to be a valid latitude and longitude.";
        }

        string[] latLongArray = latLong.Split(',');
        string lat = latLongArray[0];
        string lon = latLongArray[1];

        string url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m,apparent_temperature,is_day,precipitation,rain,showers,snowfall,weather_code,cloud_cover&daily=weather_code,apparent_temperature_max,apparent_temperature_min,precipitation_sum,rain_sum,showers_sum,snowfall_sum,precipitation_probability_max&temperature_unit=fahrenheit&wind_speed_unit=mph&precipitation_unit=inch&timezone=America%2FNew_York";

        string json = await GetJsonFromRestRequestAsync(url);
        WeatherResponse? response = JsonConvert.DeserializeObject<WeatherResponse>(json);
        CurrentWeather? current = response?.Current;

        if (current == null)
        {
            return $"Could not interpret weather data: {json}";
        }

        StringBuilder sb = new();

        string? weatherName = CurrentWeatherCode(current.WeatherCode);
        if (!string.IsNullOrEmpty(weatherName))
        {
            sb.Append($"Current weather: {weatherName}. ");
        }

        if (!current.IsDay)
        {
            sb.Append("It is dark outside. ");
        }

        sb.Append($"The current temperature feels like {current.ApparentTemperature}\u00b0F. Cloud cover is currently {current.CloudCoverPercent:P}");

        if (current.Snowfall > 0)
        {
            sb.Append(" It is currently snowing.");
        } 
        else if (current.Showers > 0)
        {
            sb.Append(" There are rain showers outside.");
        } 
        else if (current.Rain > 0)
        {
            sb.Append(" It is currently raining");
        }

        return sb.ToString();
    }

    private static string? CurrentWeatherCode(int weatherCode) 
        => weatherCode switch
        {
            0 => "Clear skies",
            1 => "Mainly clear",
            2 => "Partly cloudy",
            3 => "Overcast",
            45 => "Fog",
            48 => "Depositing rime Fog",
            51 => "Light drizzle",
            53 => "Moderate drizzle",
            55 => "Dense drizzle",
            56 => "Light freezing drizzle",
            57 => "Dense freezing drizzle",
            61 => "Slight rain",
            63 => "Moderate rain",
            65 => "Heavy rain",
            66 => "Light freezing rain",
            67 => "Heavy freezing rain",
            71 => "Slight snow fall",
            73 => "Moderate snow fall",
            75 => "Heavy snow fall",
            77 => "Snow grains",
            80 => "Slight rain showers",
            81 => "Moderate rain showers",
            82 => "Violent rain showers",
            85 => "Slight snow showers",
            86 => "Heavy snow showers",
            95 => "Thunderstorm",
            96 => "Thunderstorm with light hail",
            99 => "Thunderstorm with heavy hail",
            _ => null
        };
}