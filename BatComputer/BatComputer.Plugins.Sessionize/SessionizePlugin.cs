using Azure;
using MattEland.BatComputer.Abstractions;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using MattEland.BatComputer.Plugins.Sessionize.Model;
using Newtonsoft.Json;
using System;

namespace MattEland.BatComputer.Plugins.Sessionize;

public class SessionizePlugin : IDisposable
{
    private readonly IAppKernel _kernel;
    private readonly SessionizeService _sessionize;

    public SessionizePlugin(IAppKernel kernel, string apiToken)
    {
        _kernel = kernel;
        _sessionize = new SessionizeService(apiToken);
    }

    [SKFunction, Description("Gets data for all speakers at the conference")]
    public async Task<string> GetAllSpeakerJson()
    {
        IEnumerable<Speaker> speakers = await _sessionize.GetSpeakerEntriesAsync();

        return JsonConvert.SerializeObject(speakers);
    }

    [SKFunction, Description("Gets the names of all speakers for the conference")]
    public async Task<string> GetAllSpeakerNames()
    {
        IEnumerable<SpeakerWallEntry> speakers = await _sessionize.GetSpeakerWallEntriesAsync();

        return string.Join(", ", speakers.OrderBy(s => s.FullName).Select(s => s.FullName));
    }

    [SKFunction, Description("Gets the names of all sessions for the conference")]
    public async Task<string> GetAllSessionNames()
    {
        IEnumerable<Session> sessions = await _sessionize.GetSessionsAsync();

        return string.Join(", ", sessions.OrderBy(s => s.StartsAt).ThenBy(s => s.Title).Select(s => s.Title));
    }

    [SKFunction, Description("Gets the names of all sessions active at a specified DateTime")]
    public async Task<string> GetAllActiveSessionNames([Description("The date and time of the session")] string dateTime)
    {
        if (!DateTime.TryParse(dateTime, out DateTime activeDateTime))
        {
            return $"{dateTime} is not a valid date / time";
        }

        IEnumerable<Session> sessions = (await _sessionize.GetActiveSessionsAsync(activeDateTime)).ToList();

        if (!sessions.Any())
            return $"There are no active sessions at {dateTime}";

        return "Active sessions during this time are " + string.Join(", ",
            sessions.OrderBy(s => s.StartsAt).ThenBy(s => s.Title).Select(s => s.Title));
    }

    [SKFunction, Description("Gets the unique dates of the conference")]
    public async Task<string> GetAllSessionDates()
    {
        IEnumerable<DateTime> dates = await _sessionize.GetUniqueDatesAsync();

        return string.Join(", ", dates.Select(d => d.ToShortDateString()));
    }

    [SKFunction, Description("Gets the unique session start times from any day of the conference")]
    public async Task<string> GetAllSessionStartTimes()
    {
        IEnumerable<DateTime> dates = await _sessionize.GetUniqueStartTimesAsync();

        return string.Join(", ", dates.Select(d => d.ToString("f")));
    }

    [SKFunction, Description("Gets the unique session start times for a specific day of the conference")]
    public async Task<string> GetAllSessionStartTimesForSpecificDate([Description("The date to retrieve session start times for. For example, 1/9/24")] string date)
    {
        if (!DateTime.TryParse(date, out DateTime dateTime))
        {
            return $"{date} is not a valid date";
        }

        IEnumerable<DateTime> dates = await _sessionize.GetUniqueStartTimesAsync(dateTime);

        return string.Join(", ", dates.Select(d => d.ToString("f")));
    }

    [SKFunction, Description("Gets the names of all upcoming sessions")]
    public async Task<string> GetUpcomingSessionNames()
    {
        IEnumerable<Session> sessions = (await _sessionize.GetUpcomingSessionsAsync()).ToList();

        if (!sessions.Any())
            return "There are no upcoming sessions";

        return "Upcoming sessions are " + string.Join(", ", sessions.OrderBy(s => s.StartsAt).ThenBy(s => s.Title).Select(s => s.Title));
    }

    [SKFunction, Description("Gets the names of all completed sessions")]
    public async Task<string> GetCompletedSessionNames()
    {
        IEnumerable<Session> sessions = (await _sessionize.GetCompletedSessionsAsync()).ToList();

        if (!sessions.Any())
            return "There are no completed sessions";

        return "Completed sessions are " + string.Join(", ", sessions.OrderBy(s => s.StartsAt).ThenBy(s => s.Title).Select(s => s.Title));
    }

    [SKFunction, Description("Gets the names of all sessions in a specific room")]
    public async Task<string> GetSessionsByRoom([Description("The name of the room. For example, River A")] string room)
    {
        IEnumerable<Session> sessions = (await _sessionize.GetSessionsByRoomAsync(room)).ToList();

        if (!sessions.Any())
            return $"Could not find any sessions in room '{room}'";

        return $"Sessions in {room}: " + string.Join(", ", sessions.OrderBy(s => s.StartsAt).ThenBy(s => s.Title).Select(s => s.Title));
    }

    [SKFunction, Description("Gets the names of all sessions matching a specific category name")]
    public async Task<string> GetSessionsByCategory([Description("The name of the category. For example, AI")] string category)
    {
        IEnumerable<Session> sessions = (await _sessionize.GetSessionsByCategoryItem(category)).ToList();

        if (!sessions.Any())
            return $"Could not find any sessions with category '{category}'";

        return $"Sessions in category {category}: " + string.Join(", ", sessions.OrderBy(s => s.StartsAt).ThenBy(s => s.Title).Select(s => s.Title));
    }

    [SKFunction, Description("Gets the names of all categories in the conference")]
    public async Task<string> GetUniqueCategories()
    {
        IEnumerable<string> categories = (await _sessionize.GetUniqueCategoriesAsync()).ToList();

        if (!categories.Any())
            return "Could not find any categories";

        return "Available categories are: " + string.Join(", ", categories);
    }

    [SKFunction, Description("Gets the names of all rooms in the conference")]
    public async Task<string> GetUniqueRooms()
    {
        IEnumerable<string> rooms = (await _sessionize.GetUniqueRoomsAsync()).ToList();

        if (!rooms.Any())
            return "Could not find any rooms";

        return "Available rooms are: " + string.Join(", ", rooms);
    }

    [SKFunction, Description("Gets the session names of sessions by a specific speaker")]
    public async Task<string> GetSessionNamesBySpeaker([Description("The full name of the speaker")] string fullName)
    {
        IEnumerable<Session> sessions = (await _sessionize.GetSessionsBySpeakerAsync(fullName)).ToList();

        if (!sessions.Any())
            return $"There are no sessions by {fullName}";

        return $"{fullName}'s sessions are " + string.Join(", ", sessions.OrderBy(s => s.StartsAt).ThenBy(s => s.Title).Select(s => s.Title));
    }

    [SKFunction, Description("Gets session details by a session name")]
    public async Task<string> GetSessionDetails([Description("The name of the session")] string sessionName)
    {
        Session? session = await _sessionize.GetSessionByTitleAsync(sessionName);

        if (session == null)
            return $"Could not find a session named '{sessionName}'";

        // TODO: This probably deserves a widget

        return JsonConvert.SerializeObject(session); // TODO: Maybe not JSON?
    }

    [SKFunction, Description("Gets speaker details by their full name")]
    public async Task<string> GetSpeakerDetails([Description("The full name of the speaker")] string speakerName)
    {
        Speaker? speaker = await _sessionize.GetSpeakerByFullName(speakerName);

        if (speaker == null)
            return $"Could not find a speaker named '{speakerName}'";

        // TODO: This probably deserves a widget

        return JsonConvert.SerializeObject(speaker); // TODO: Maybe not JSON?
    }

    [SKFunction, Description("Gets data for all sessions at the conference")]
    public async Task<string> GetAllSessionJson()
    {
        IEnumerable<Session> sessions = await _sessionize.GetSessionsAsync();

        return JsonConvert.SerializeObject(sessions);
    }

    // TODO: A search speakers would be nice

    // TODO: A search sessions would be nice

    public void Dispose()
    {
        _sessionize.Dispose();
    }
}
