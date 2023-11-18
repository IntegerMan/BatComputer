using System.Text.Json.Serialization;

namespace MattEland.BatComputer.Abstractions.Strategies;

public class StepwiseSummary
{
    public string? Thought { get; set; }
    public string? Action { get; set; }
    [JsonPropertyName("action_variables")]
    public Dictionary<string, string> ActionVariables { get; set; } = new();
    public string? Observation { get; set; }
    [JsonPropertyName("final_answer")]
    public string? FinalAnswer { get; set; }
    [JsonPropertyName("original_response")]
    public string? OriginalResponse { get; set; }
}

/*
[{"thought":"To describe the image, I can use the Vision.AnalyzeDiskImage function to analyze the image and
get a list of detected
objects.","action":"Vision.AnalyzeDiskImage","action_variables":{"filePath":"C:\\Users\\Admin\\AppData\\Local\\Temp\\vsa
tpfjq.png"},"observation":"Analyzed an image described as: a man wearing headphones and sitting in a chair.\r\nIn this
image the following objects were detected: Goggles, person.\r\nText in the image: There\u0027s 1270 TUESDAY WEDNESDAY
THURSES SATURDAY.\r\n","final_answer":"","original_response":"[THOUGHT]\nTo describe the image, I can use the
Vision.AnalyzeDiskImage function to analyze the image and get a list of detected objects.\n\n[ACTION]\n{\n
\u0022action\u0022: \u0022Vision.AnalyzeDiskImage\u0022,\n  \u0022action_variables\u0022: {\n    \u0022filePath\u0022:
\u0022C:\\\\Users\\\\Admin\\\\AppData\\\\Local\\\\Temp\\\\vsatpfjq.png\u0022\n
}\n}"},{"thought":"","action":"","action_variables":{},"observation":"","final_answer":"The image at
C:\\Users\\Admin\\AppData\\Local\\Temp\\vsatpfjq.png depicts a man wearing headphones and sitting in a chair. The image
also contains the following objects: goggles and a person. Additionally, there is text in the image that reads:
\u0022There\u0027s 1270 TUESDAY WEDNESDAY THURSES SATURDAY.\u0022","original_response":"[FINAL ANSWER] The image at
C:\\Users\\Admin\\AppData\\Local\\Temp\\vsatpfjq.png depicts a man wearing headphones and sitting in a chair. The image
also contains the following objects: goggles and a person. Additionally, there is text in the image that reads:
\u0022There\u0027s 1270 TUESDAY WEDNESDAY THURSES SATURDAY.\u0022"}]*/