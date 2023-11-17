using Newtonsoft.Json;

namespace MattEland.BatComputer.Plugins.Sessionize.Model;

public class SpeakerWallEntry
{
    [JsonProperty("id")]
    public Guid Id { get; set; }

    [JsonProperty("firstName")]
    public required string FirstName { get; set; }

    [JsonProperty("lastName")]
    public required string LastName { get; set; }

    [JsonProperty("fullName")]
    public required string FullName { get; set; }

    [JsonProperty("tagLine")]
    public required string TagLine { get; set; }

    [JsonProperty("profilePicture")]
    public required string ProfilePicture { get; set; }

    [JsonProperty("isTopSpeaker")]
    public bool IsTopSpeaker { get; set; }
}
