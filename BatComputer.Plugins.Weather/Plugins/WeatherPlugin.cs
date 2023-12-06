using System.ComponentModel;
using System.Text;
using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.Plugins.Weather.Models;
using MattEland.BatComputer.Plugins.Weather.Widgets;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;

namespace MattEland.BatComputer.Plugins.Weather.Plugins;

public class WeatherPlugin : OpenMeteoPlugin
{

    [SKFunction, Description("Gets current weather conditions from a latitude and longitude. This should not be used for predicting weather.")]
    public async Task<string> GetCurrentWeatherFromLatLong(
        [Description("The latitude and longitude. Formatted like: 39.961,-82.998 where 39.961 is the latitude and -82.998 is the longitude. This cannot be a zip code or city name.")] string latLong)
    {
        if (!latLong.Contains(','))
        {
            return $"{latLong} does not appear to be a valid latitude and longitude.";
        }

        (string lat, string lon) = GetLatLongFromString(latLong);

        WeatherResponse response = await GetWeatherInformationAsync(lat, lon);
        CurrentWeather? current = response.Current;
        if (current == null)
        {
            return $"Could not get current weather for lat/long {latLong}";
        }

        StringBuilder sb = new();

        CurrentWeatherWidget widget = new()
        {
            Title = "Current Weather",
            Temperature = $"{current.Temperature}°F",
            ApparentTemperature = $"{current.ApparentTemperature}°F",
            CloudCover = $"{current.CloudCoverPercent} %",
            IsDay = current.IsDay,
            Rainfall = current.Rain > 0 ? $"{current.Rain:0.##} inches" : "None",
            Snowfall = current.Snowfall > 0 ? $"{current.Snowfall:0.##} inches" : "None"
        };

        string? weatherName = CurrentWeatherCode(current.WeatherCode);
        if (!string.IsNullOrEmpty(weatherName))
        {
            sb.Append($"Current weather: {weatherName}. ");
            widget.Conditions = weatherName;
        }

        if (!current.IsDay)
        {
            sb.Append("It is dark outside. ");
        }

        sb.Append($"The current temperature is {current.Temperature}°F and feels like {current.ApparentTemperature}°F. " +
            $"Cloud cover is currently {current.CloudCoverPercent} %");

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

        // TODO: Kernel.AddWidget(widget);

        return sb.ToString();
    }

    private static (string lat, string lon) GetLatLongFromString(string latLong)
    {
        string[] latLongArray = latLong.Split(',');
        string lat = latLongArray[0];
        string lon = latLongArray[1];
        return (lat, lon);
    }

    /*

    [SKFunction, Description("Gets weather information from a latitude and longitude for tomorrow")]
    public async Task<string> GetTomorrowWeatherFromLatLong(
        [Description("The latitude and longitude. Formatted like: 39.961,-82.998 where 39.961 is the latitude and -82.998 is the longitude. This cannot be a zip code or city name.")] string latLong)
    {
        if (!latLong.Contains(','))
        {
            return latLong + " does not appear to be a valid latitude and longitude.";
        }

        (string lat, string lon) = GetLatLongFromString(latLong);

        WeatherResponse response = await GetWeatherInformationAsync(lat, lon);
        DailyWeather? forecast = response.Daily;

        if (forecast == null)
        {
            return $"Could not get a forecast for lat {lat}, long{lon}";
        }

        int index = 1; // 0 is always today, so 1 is tomorrow

        return BuildWeatherDayForecastString(forecast, index);
    }
    */

    private static string BuildWeatherDayForecastString(DailyWeather forecast, int index)
    {
        StringBuilder sb = new();

        string? weatherName = CurrentWeatherCode(forecast.WeatherCode[index]);
        if (!string.IsNullOrEmpty(weatherName))
        {
            sb.Append($"Predicted weather conditions on {forecast.Time[index]}: {weatherName}. ");
        }

        sb.Append($"The temperature will range between {forecast.ApparentTemperatureMin[index]}\u00b0F and {forecast.ApparentTemperatureMax[index]}\u00b0F. ");
        sb.Append($"The precipitation chance is {forecast.PrecipitationChanceMax[index]}%. ");

        AddPrecipitationNote(sb, forecast.Rain[index], "rain");
        AddPrecipitationNote(sb, forecast.Showers[index], "showers");
        AddPrecipitationNote(sb, forecast.Snowfall[index], "snow");
        AddPrecipitationNote(sb, forecast.Precipitation[index], "precipitation");

        return sb.ToString();
    }

    [SKFunction, Description("Gets weather information from a latitude and longitude for a given date")]
    public async Task<string> GetDailyWeatherFromLatLong(
        [Description("The latitude and longitude. Formatted like: 39.961,-82.998 where 39.961 is the latitude and -82.998 is the longitude. This cannot be a zip code or city name.")] string latLong,
        [Description("A string representing a calendar date such as 9/10/80 or 2023-11-13 or today or tomorrow")] string dateStr)
    {
        if (!latLong.Contains(','))
        {
            return latLong + " does not appear to be a valid latitude and longitude.";
        }

        (string lat, string lon) = GetLatLongFromString(latLong);

        WeatherResponse response = await GetWeatherInformationAsync(lat, lon);
        DailyWeather? forecast = response.Daily;

        if (forecast == null)
        {
            return $"Could not get daily weather for lat/long {lat},{lon}";
        }

        if (dateStr.Equals("today", StringComparison.OrdinalIgnoreCase))
        {
            return BuildWeatherDayForecastString(forecast, 0);
        }

        if (dateStr.Equals("tomorrow", StringComparison.OrdinalIgnoreCase))
        {
            return BuildWeatherDayForecastString(forecast, 1);
        }

        if (DateTime.TryParse(dateStr, out DateTime date))
        {
            string time = date.ToString("yyyy-MM-dd");

            int index = forecast.Time.IndexOf(time);

            return index == -1
                ? $"I couldn't get weather forecast information on {dateStr}"
                : BuildWeatherDayForecastString(forecast, index);
        }

        return $"'{dateStr}' could not be interpreted as a date. Expecting a value like '{DateTime.Today.ToShortDateString()}'";
    }

    private static void AddPrecipitationNote(StringBuilder sb, decimal amount, string precipType)
    {
        if (amount > 0)
        {
            sb.Append($"You can expect {amount} inches of {precipType}");
        }
    }

    private static async Task<WeatherResponse> GetWeatherInformationAsync(string lat, string lon)
    {
        string url = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current=temperature_2m,apparent_temperature,is_day,precipitation,rain,showers,snowfall,weather_code,cloud_cover&daily=weather_code,apparent_temperature_max,apparent_temperature_min,precipitation_sum,rain_sum,showers_sum,snowfall_sum,precipitation_probability_max&temperature_unit=fahrenheit&wind_speed_unit=mph&precipitation_unit=inch&timezone=America%2FNew_York";

        string json = await GetJsonFromRestRequestAsync(url);
        WeatherResponse? response = JsonConvert.DeserializeObject<WeatherResponse>(json);

        if (response == null)
        {
            throw new JsonSerializationException($"Could not convert {json} into a WeatherResponse");
        }

        return response;
    }

    // TODO: There's probably a library of images for these weather codes that I could use

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