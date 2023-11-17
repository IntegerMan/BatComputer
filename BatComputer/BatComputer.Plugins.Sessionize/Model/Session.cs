using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MattEland.BatComputer.Plugins.Sessionize.Model;

public class Session
{
    /*
    [JsonProperty("questionAnswers")]
    public List<object> QuestionAnswers { get; set; }
    */

    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("title")]
    public required string Title { get; set; }

    [JsonProperty("description")]
    public string? Description { get; set; }

    [JsonProperty("startsAt")]
    public DateTime StartsAt { get; set; }

    [JsonProperty("endsAt")]
    public DateTime EndsAt { get; set; }

    [JsonProperty("isServiceSession")]
    public bool IsServiceSession { get; set; }

    [JsonProperty("isPlenumSession")]
    public bool IsPlenumSession { get; set; }

    [JsonProperty("speakers")] 
    public List<SessionSpeaker> Speakers { get; set; } = new();

    [JsonProperty("categories")] 
    public List<Category> Categories { get; set; } = new();

    [JsonProperty("roomId")]
    public int RoomId { get; set; }

    [JsonProperty("room")]
    public required string Room { get; set; }

    [JsonProperty("liveUrl")]
    public string? LiveUrl { get; set; }

    [JsonProperty("recordingUrl")]
    public string? RecordingUrl { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; } = "Accepted";
}