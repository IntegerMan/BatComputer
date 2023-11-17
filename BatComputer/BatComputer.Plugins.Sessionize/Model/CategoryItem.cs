using Newtonsoft.Json;

namespace MattEland.BatComputer.Plugins.Sessionize.Model;

public class CategoryItem
{
    [JsonProperty("id")]
    public long Id { get; set; }

    [JsonProperty("name")]
    public required string Name { get; set; }
}