using Azure;
using MattEland.BatComputer.Abstractions;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using MattEland.BatComputer.Plugins.Sessionize.Model;
using Newtonsoft.Json;

namespace MattEland.BatComputer.Plugins.Sessionize;

public class SessionizePlugin
{
    private readonly IAppKernel _kernel;
    private readonly string _apiToken;

    public SessionizePlugin(IAppKernel kernel, string apiToken)
    {
        _kernel = kernel;
        _apiToken = apiToken;
    }

    [SKFunction, Description("Gets high level JSON for all speakers at the conference")]
    public async Task<string> GetAllSpeakerJson()
    {
        HttpClient client = new();

        string json = await client.GetStringAsync($"https://sessionize.com/api/v2/{_apiToken}/view/SpeakerWall");

        return json;
    }

    [SKFunction, Description("Gets the names of all speakers for the conference")]
    public async Task<string> GetAllSpeakerNames()
    {
        string json = await GetAllSpeakerJson();

        List<SpeakerWallEntry> speakers = JsonConvert.DeserializeObject<List<SpeakerWallEntry>>(json)!;

        return string.Join(", ", speakers.OrderBy(s => s.FullName).Select(s => s.FullName));
    }

}
