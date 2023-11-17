using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime;
using System.Text;

namespace MattEland.BatComputer.Kernel;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

public class KernelSettings {
    public string AzureAiServicesEndpoint { get; set; }
    public string AzureAiServicesKey { get; set; }
    public string AzureAiServicesRegion { get; set; }

    public string AzureOpenAiEndpoint { get; set; }
    public string AzureOpenAiKey { get; set; }

    public string OpenAiDeploymentName { get; set; }

    /// <summary>
    /// The token for a Sessionize URL
    /// </summary>
    /// <example>
    /// For https://sessionize.com/api/v2/abcd/view/SpeakerWall the token would be abcd
    /// </example>
    public string? SessionizeToken { get; set; }
    public string SpeechVoiceName { get; set; } = "en-GB-AlfieNeural";

    public string? BingKey { get; set; }

    public void Validate()
    {
        List<ValidationResult> results = Validate(new ValidationContext(this)).ToList();

        if (results.Count > 0)
        {
            StringBuilder sb = new();
            sb.AppendLine("Settings were in an invalid state after all settings were parsed:");
            foreach (ValidationResult violation in results)
            {
                sb.AppendLine($"- {violation.ErrorMessage}");
            }

            throw new InvalidOperationException(sb.ToString());
        }
    }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        List<ValidationResult> results = new();

        Validator.TryValidateObject(this, validationContext, results);

        return results;
    }

    public bool IsValid => !Validate(new ValidationContext(this)).Any();
    public bool SupportsSearch => !string.IsNullOrWhiteSpace(BingKey);
    public bool SupportsAiServices => !string.IsNullOrWhiteSpace(AzureAiServicesEndpoint) && !string.IsNullOrWhiteSpace(AzureAiServicesKey) && !string.IsNullOrWhiteSpace(AzureAiServicesRegion);
    public bool SupportsSessionize => !string.IsNullOrWhiteSpace(SessionizeToken);
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
