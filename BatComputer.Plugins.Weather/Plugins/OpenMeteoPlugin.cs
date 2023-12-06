namespace MattEland.BatComputer.Plugins.Weather.Plugins;

public abstract class OpenMeteoPlugin
{
    protected OpenMeteoPlugin()
    {
    }

    protected static async Task<string> GetJsonFromRestRequestAsync(string url)
    {
        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }
}