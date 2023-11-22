using MattEland.BatComputer.Plugins.Sessionize.Model;
using Newtonsoft.Json;

namespace MattEland.BatComputer.Plugins.Sessionize;

public class SessionizeService : IDisposable
{
    private readonly HttpClient _client;
    private readonly string _apiToken;

    public SessionizeService(string apiToken)
    {
        _apiToken = apiToken;
        _client = new HttpClient(); // TODO: Use IHttpClientFactory
    }

    public async Task<IEnumerable<SpeakerWallEntry>> GetSpeakerWallEntriesAsync()
    {
        string json = await _client.GetStringAsync($"https://sessionize.com/api/v2/{_apiToken}/view/SpeakerWall");

        List<SpeakerWallEntry> speakers = JsonConvert.DeserializeObject<List<SpeakerWallEntry>>(json)!;

        return speakers;
    }

    public async Task<IEnumerable<Speaker>> GetSpeakerEntriesAsync()
    {
        string json = await _client.GetStringAsync($"https://sessionize.com/api/v2/{_apiToken}/view/Speakers");

        List<Speaker> speakers = JsonConvert.DeserializeObject<List<Speaker>>(json)!;

        return speakers;
    }

    public async Task<IEnumerable<Session>> GetSessionsAsync()
    {
        string json = await _client.GetStringAsync($"https://sessionize.com/api/v2/{_apiToken}/view/Sessions");

        List<SessionGroup> sessions = JsonConvert.DeserializeObject<List<SessionGroup>>(json)!;

        return sessions.SelectMany(g => g.Sessions);
    }    

    public void Dispose() => _client.Dispose();
}