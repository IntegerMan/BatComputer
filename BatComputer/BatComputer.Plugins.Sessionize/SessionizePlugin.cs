using MattEland.BatComputer.Plugins.Sessionize.Model;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using System.ComponentModel;
using System.Text;

namespace MattEland.BatComputer.Plugins.Sessionize;

public class SessionizePlugin : IDisposable
{
    private readonly ISemanticTextMemory? _memory;
    private readonly SessionizeService _sessionize;
    private readonly List<Session> _sessions = new();
    private readonly List<Speaker> _speakers = new();

    public SessionizePlugin(ISemanticTextMemory? memory, string apiToken, string? collectionName = null)
    {
        _memory = memory;
        _sessionize = new SessionizeService(apiToken);
        SessionsMemoryCollection = collectionName ?? "Sessionize";
    }

    public string SessionsMemoryCollection { get; set; }

    private async Task<List<Session>> GetSessionsAsync()
    {
        if (_sessions.Count <= 0)
        {
            DateTime now = DateTime.UtcNow;
            string additionalMetadata = $"Session retrieved {now}";

            IEnumerable<Session> sessions = await _sessionize.GetSessionsAsync();

            foreach (Session session in sessions)
            {
                if (_memory != null)
                {
                    string text = BuildSessionString(session);
                    string description = $"Session '{session.Title}'";

                    if (session.Speakers.Count == 1)
                    {
                        description += $" by {session.Speakers.First().Name}";
                    }
                    else if (session.Speakers.Count > 1)
                    {
                        description += $" by {string.Join(", ", session.Speakers.Select(s => s.Name))}";
                    }

                    await _memory.SaveInformationAsync(SessionsMemoryCollection, text, session.Id, description, additionalMetadata);
                }

                _sessions.Add(session);
            }
        }

        return _sessions;
    }

    private async Task<List<Speaker>> GetSpeakersAsync()
    {
        if (_speakers.Count <= 0)
        {
            DateTime now = DateTime.UtcNow;
            string additionalMetadata = $"Speaker retrieved {now}";

            IEnumerable<Speaker> speakers = await _sessionize.GetSpeakerEntriesAsync();

            foreach (Speaker speaker in speakers)
            {
                if (_memory != null)
                {
                    string text = BuildSpeakerString(speaker);
                    string description = $"Speaker {speaker.FullName}";

                    if (speaker.Sessions.Count > 0)
                    {
                        description += $" speaking on {string.Join(", ", speaker.Sessions.Select(s => $"'{s.Name}'"))}";
                    }

                    await _memory.SaveInformationAsync(SessionsMemoryCollection, text, speaker.Id, description, additionalMetadata);
                }

                _speakers.Add(speaker);
            }
        }

        return _speakers;
    }

    [SKFunction, Description("Searches the sessions and speakers in memory for relevant information")]
    public async Task<string> Search([SKName("query"), Description("Provide a long and verbose sentence about the speaker, session, or topic of interest")] string query)
    {
        if (_memory == null)
        {
            return "I can't search sessions without a memory configured.";
        }

        IAsyncEnumerable<MemoryQueryResult> results = 
            _memory.SearchAsync(SessionsMemoryCollection, query, limit: 5, minRelevanceScore: 0.5);

        StringBuilder sb = new();

        await foreach (MemoryQueryResult result in results)
        {
            string id = result.Metadata.Id;

            Session? session = _sessions.FirstOrDefault(s => s.Id == id);
            Speaker? speaker = _speakers.FirstOrDefault(s => s.Id == id);

            if (session != null)
            {
                sb.AppendLine(BuildSessionString(session));
            }
            else if (speaker != null)
            {
                sb.AppendLine(BuildSpeakerString(speaker));
            }
        }

        return sb.Length == 0
            ? $"I couldn't find any sessions or speakers related to '{query}'"
            : $"I found the following sessions and speakers related to that: \r\n{sb}";
    }

    [SKFunction, Description("Gets the names of all speakers for the conference")]
    public async Task<string> GetAllSpeakerNames()
    {
        IEnumerable<Speaker> speakers = await GetSpeakersAsync();

        return string.Join(", ", speakers.OrderBy(s => s.FullName).Select(s => s.FullName).Distinct());
    }

    [SKFunction, Description("Gets the title of all sessions for the conference")]
    public async Task<string> GetAllSessions()
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();

        return string.Join(", ", sessions.OrderBy(s => s.StartsAt).ThenBy(s => s.Title).Select(s => s.Title));
    }

    /*
    [SKFunction, Description("Gets the titles of all sessions active at a specified time")]
    public async Task<string> GetAllActiveSessionNames([Description("The date and time of the session")] string dateTime)
    {
        if (!DateTime.TryParse(dateTime, out DateTime activeDateTime))
        {
            return $"{dateTime} is not a valid date / time";
        }

        IEnumerable<Session> sessions = (await GetSessionsAsync()).Where(s => s.StartsAt <= activeDateTime && s.EndsAt >= activeDateTime).ToList();

        if (!sessions.Any())
            return $"There are no active sessions at {dateTime}";

        return "Active sessions during this time are " + string.Join(", ",
            sessions.OrderBy(s => s.StartsAt).ThenBy(s => s.Title).Select(s => s.Title));
    }

    [SKFunction, Description("Gets the unique dates of the conference")]
    public async Task<string> GetAllSessionDates()
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();
        IEnumerable<DateTime> dates = sessions.Select(s => s.StartsAt.Date).Distinct().OrderBy(d => d);

        return string.Join(", ", dates.Select(d => d.ToShortDateString()));
    }

    [SKFunction, Description("Gets the unique session start times")]
    public async Task<string> GetAllSessionStartTimes()
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();
        IEnumerable<DateTime> dates = sessions.Select(s => s.StartsAt).Distinct().OrderBy(d => d);

        return string.Join(", ", dates.Select(d => d.ToString("f")));
    }

    [SKFunction, Description("Gets the unique session start times for a specific day of the conference")]
    public async Task<string> GetAllSessionStartTimesForSpecificDate(
        [Description("The date to retrieve session start times for. For example, 1/9/24")] string date)
    {
        if (!DateTime.TryParse(date, out DateTime dateTime))
        {
            return $"{date} is not a valid date";
        }

        IEnumerable<Session> sessions = await GetSessionsAsync();
        IEnumerable<DateTime> dates = sessions.Select(s => s.StartsAt).Distinct().OrderBy(d => d);

        return string.Join(", ", dates.Select(d => d.ToString("f")));
    }

    [SKFunction, Description("Gets the titles of all upcoming sessions")]
    public async Task<string> GetUpcomingSessionTitles()
    {
        IEnumerable<Session> sessions = (await GetSessionsAsync()).Where(s => s.StartsAt >= DateTime.Now);
        
        if (!sessions.Any())
            return "There are no upcoming sessions";

        return $"Upcoming sessions are {string.Join(", ", sessions.OrderBy(s => s.StartsAt).ThenBy(s => s.Title).Select(s => s.Title))}";
    }
    */

    [SKFunction, Description("Gets the title of all completed sessions")]
    public async Task<string> GetCompletedSessionTitles()
    {
        IEnumerable<Session> sessions = (await GetSessionsAsync()).Where(s => s.EndsAt <= DateTime.Now);

        if (!sessions.Any())
            return "There are no completed sessions";

        return $"Completed sessions are {string.Join(", ", sessions.OrderBy(s => s.StartsAt).ThenBy(s => s.Title).Select(s => s.Title))}";
    }

    [SKFunction, Description("Gets the title of all sessions in a specific room")]
    public async Task<string> GetSessionNamesByRoom([Description("The name of the room. For example, River A")] string room)
    {
        IEnumerable<Session> sessions = (await GetSessionsAsync())
            .Where(s => s.Room.Equals(room, StringComparison.OrdinalIgnoreCase));

        if (!sessions.Any())
            return $"Could not find any sessions in room '{room}'";

        return $"Sessions in {room}: {string.Join(", ", sessions.OrderBy(s => s.StartsAt).ThenBy(s => s.Title).Select(s => s.Title))}";
    }

    /*
    [SKFunction, Description("Searches the sessions")]
    public async Task<string> Search([Description("The topic to search for")] string query)
    {
        if (_memory == null)
        {
            return "I can't search sessions without memory capabilities.";
        }

        IEnumerable<Session> sessions = await GetSessionsAsync();
        IAsyncEnumerable<MemoryQueryResult> results = _memory.SearchAsync(SessionsMemoryCollection, query, limit: 5, minRelevanceScore: 0.5);

        StringBuilder sb = new();

        await foreach (MemoryQueryResult result in results)
        {
            string id = result.Metadata.Id;

            Session? session = sessions.FirstOrDefault(s => s.Id == id);

            if (session == null)
            {
                continue;
            }

            sb.AppendLine($"- {session.Title}");
        }

        if (sb.Length == 0)
        {
            return $"I couldn't find any sessions related to '{query}'";
        }

        return "I found the following sessions: \r\n" + sb.ToString();
    }
    */

    [SKFunction, Description("Gets the names of all rooms in the conference")]
    public async Task<string> GetUniqueRooms()
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();
        IEnumerable<string> rooms = sessions.Select(s => s.Room).Distinct();

        if (!rooms.Any())
            return "Could not find any rooms";

        return $"Available rooms are: {string.Join(", ", rooms)}";
    }

    [SKFunction, Description("Gets the session titles of sessions by a specific speaker")]
    public async Task<string> GetSessionsBySpeaker([Description("The full name of the speaker")] string fullName)
    {
        IEnumerable<Session> sessions = (await GetSessionsAsync()).Where(s => s.Speakers.Exists(sp => sp.Name.Equals(fullName, StringComparison.OrdinalIgnoreCase)));

        if (!sessions.Any())
            return $"There are no sessions by {fullName}";

        return $"{fullName}'s sessions are {string.Join(", ", sessions.OrderBy(s => s.StartsAt).ThenBy(s => s.Title).Select(s => s.Title))}";
    }

    [SKFunction, Description("Gets session details by a session title")]
    public async Task<string> GetSessionDetails([Description("The title of the session")] string title)
    {
        IEnumerable<Session> sessions = await GetSessionsAsync();
        Session? session = sessions.FirstOrDefault(s => string.Equals(s.Title, title, StringComparison.OrdinalIgnoreCase));

        if (session == null)
        {
            return $"Could not find a session named '{title}'";
        }

        return BuildSessionString(session);
    }

    private static string BuildSessionString(Session session)
    {
        StringBuilder sb = new();
        bool isPast = session.EndsAt < DateTime.Now;
        
        if (session.Speakers.Count == 1)
        {
            sb.Append($"{session.Speakers.First().Name} {(isPast ? "spoke on " : "is speaking on ")}");
        }
        else if (session.Speakers.Count > 1)
        {
            sb.Append($"{string.Join(", ", session.Speakers.Select(s => s.Name))} {(isPast ? "spoke on " : "are speaking on ")}");
        }

        sb.Append($"'{session.Title}' in room {session.Room}");
        sb.Append($" from {session.StartsAt.ToLocalTime().ToShortTimeString()} to {session.EndsAt.ToLocalTime().ToShortTimeString()}");
        sb.Append($" on {session.StartsAt.Date.ToShortDateString()}.");

        List<string> tags = session.Categories.SelectMany(c => c.CategoryItems).Select(t => t.Name).ToList();
        if (tags.Count > 0)
        {
            sb.Append($" The session is tagged with the following categories: {string.Join(", ", tags)}.");
        }
        sb.AppendLine($" The session's abstract follows:");
        sb.AppendLine(session.Description);

        return sb.ToString();
    }

    [SKFunction, Description("Gets speaker details by their full name")]
    public async Task<string> GetSpeakerDetails([Description("The full name of the speaker")] string fullName)
    {
        IEnumerable<Speaker> speakers = await GetSpeakersAsync();
        Speaker? speaker = speakers.FirstOrDefault(s => string.Equals(s.FullName, fullName, StringComparison.OrdinalIgnoreCase));

        if (speaker == null)
        {
            return $"Could not find a speaker named '{fullName}'";
        }

        return BuildSpeakerString(speaker);
    }

    private static string BuildSpeakerString(Speaker speaker) 
        => $"{speaker.FullName} is speaking on the following sessions: {string.Join(", ", speaker.Sessions.Select(s => s.Name))}. Their bio follows: \r\n{speaker.Bio}";

    public void Dispose()
    {
        _sessionize.Dispose();
    }
}
