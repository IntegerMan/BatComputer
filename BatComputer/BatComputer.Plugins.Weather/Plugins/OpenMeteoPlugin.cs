using MattEland.BatComputer.Abstractions;

namespace BatComputer.Plugins.Weather.Plugins;

public abstract class OpenMeteoPlugin
{
    public IAppKernel Kernel { get; }

    protected OpenMeteoPlugin(IAppKernel kernel)
    {
        Kernel = kernel;
    }

    protected static async Task<string> GetJsonFromRestRequestAsync(string url)
    {
        using HttpClient client = new();
        HttpResponseMessage response = await client.GetAsync(url);
        return await response.Content.ReadAsStringAsync();
    }
}