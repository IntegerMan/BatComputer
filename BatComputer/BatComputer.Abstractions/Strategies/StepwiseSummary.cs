using System.Text.Json.Serialization;

namespace MattEland.BatComputer.Abstractions.Strategies;

public class StepwiseSummary
{
    public string? Thought { get; set; }
    public string? Action { get; set; }
    [JsonPropertyName("action_variables")]
    public Dictionary<string, string?> ActionVariables { get; set; } = new();
    public string? Observation { get; set; }
    [JsonPropertyName("final_answer")]
    public string? FinalAnswer { get; set; }
    [JsonPropertyName("original_response")]
    public string? OriginalResponse { get; set; }
}
