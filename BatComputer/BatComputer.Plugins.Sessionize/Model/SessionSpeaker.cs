using Newtonsoft.Json;

namespace MattEland.BatComputer.Plugins.Sessionize.Model;

public class SessionSpeaker
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public required string Name { get; set; }
}