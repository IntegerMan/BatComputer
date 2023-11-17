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
        _client = new HttpClient();
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
    
    public async Task<Speaker?> GetSpeakerByFullName(string fullName)
    {
        IEnumerable<Speaker> speakers = await GetSpeakerEntriesAsync();

        return speakers.FirstOrDefault(s => string.Equals(s.FullName, fullName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Speaker?> GetSpeakerById(string id)
    {
        IEnumerable<Speaker> speakers = await GetSpeakerEntriesAsync();

        return speakers.FirstOrDefault(s => s.Id == id);
    }
    
    public async Task<Session?> GetSessionByTitleAsync(string title)
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();

        return sessions.FirstOrDefault(s => string.Equals(s.Title, title, StringComparison.OrdinalIgnoreCase));
    }
    
    public async Task<Session?> GetSessionById(string id)
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();

        return sessions.FirstOrDefault(s => s.Id == id);
    }
        
    public async Task<IEnumerable<Session>> GetSessionsBySpeakerAsync(string speaker)
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();

        return sessions.Where(s => s.Speakers.Exists(sp => sp.Name.Equals(speaker, StringComparison.OrdinalIgnoreCase)));
    }
    
    public async Task<IEnumerable<Session>> GetSessionsByCategoryItem(string item)
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();

        return sessions.Where(s => s.Categories.Exists(c => c.CategoryItems.Exists(ci => ci.Name.Equals(item, StringComparison.OrdinalIgnoreCase))));
    }
        
    public async Task<IEnumerable<Session>> GetSessionsByRoomAsync(string room)
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();

        return sessions.Where(s => s.Room.Equals(room, StringComparison.OrdinalIgnoreCase));
    }
    
    public async Task<IEnumerable<Session>> GetUpcomingSessionsAsync()
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();

        return sessions.Where(s => s.StartsAt >= DateTime.Now);
    }

    public async Task<IEnumerable<Session>> GetCompletedSessionsAsync()
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();

        return sessions.Where(s => s.EndsAt < DateTime.Now);
    }

    public async Task<IEnumerable<Session>> GetActiveSessionsAsync(DateTime activeDateTime)
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();

        return sessions.Where(s => s.StartsAt <= activeDateTime && s.EndsAt >= activeDateTime);
    }

    public async Task<IEnumerable<Session>> GetDailySessionsAsync(DateTime activeDate)
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();

        return sessions.Where(s => s.StartsAt.Date == activeDate.Date);
    }

    public async Task<IEnumerable<DateTime>> GetUniqueStartTimesAsync()
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();

        return sessions.Select(s => s.StartsAt).Distinct().OrderBy(d => d);
    }

    public async Task<IEnumerable<DateTime>> GetUniqueStartTimesAsync(DateTime activeDate)
    {
        IEnumerable<Session> sessions = await GetDailySessionsAsync(activeDate);

        return sessions.Select(s => s.StartsAt).Distinct().OrderBy(d => d);
    }

    public async Task<IEnumerable<DateTime>> GetUniqueDatesAsync()
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();

        return sessions.Select(s => s.StartsAt.Date).Distinct().OrderBy(d => d);
    }

    public async Task<IEnumerable<string>> GetUniqueCategoriesAsync()
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();
        return sessions.SelectMany(s => s.Categories).SelectMany(c => c.CategoryItems).Select(c => c.Name).Distinct().OrderBy(c => c);
    }

    public async Task<IEnumerable<string>> GetUniqueRoomsAsync()
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();
        return sessions.Select(s => s.Room).Distinct().OrderBy(s => s);
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}