using System.ComponentModel;
using System.Text.Encodings.Web;
using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.Plugins.Weather.Models;
using MattEland.BatComputer.Plugins.Weather.Widgets;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;

namespace MattEland.BatComputer.Plugins.Weather.Plugins;

public class LatLongPlugin : OpenMeteoPlugin
{
    public LatLongPlugin(IAppKernel kernel) : base(kernel)
    {
    }

    [SKFunction, Description("Gets a latitude and longitude string from a city, zip code, or location. Result is formatted like: lat,long")]
    public async Task<string> GetLatLong([Description("The city or location such as Columbus, Ohio or a Zip code like 43081")] string location)
    {

        if (location.Contains('$') || location.Contains('.'))
        {
            return $"{location} doesn't seem like a valid location. I can't find its latitude and longitude.";
        }

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
            return $"Could not interpret lat / long for {location} based on the OpenMeteo response: {json}";
        }

        GeoCodingLocation geo = geoCodingResponse.Results.First();

        Kernel.AddWidget(new LatLongWidget()
        {
            Title = $"{location} Lat / Long",
            Latitude = geo.Latitude,
            Longitude = geo.Longitude,
        });

        return geo.Latitude + "," + geo.Longitude;
    }
}