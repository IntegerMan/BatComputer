using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using OpenMeteo;

namespace BatComputer.Plugins.Weather;

public class OpenMeteoPlugin
{

    [SKFunction, Description("Gets JSON representing the current weather conditions for a given city or location")]
    public async Task<string> GetWeatherForCity([Description("The city or location, such as Columbus, Ohio")] string location)
    {
        OpenMeteoClient client = new();

        WeatherForecast? response = await client.QueryAsync(location);

        return JsonSerializer.Serialize(response);
    }

}
