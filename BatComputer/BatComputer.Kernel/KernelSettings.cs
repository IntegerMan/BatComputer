using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime;
using System.Text;

namespace MattEland.BatComputer.Kernel;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

public class KernelSettings {
    [Required]
    public string AzureAiServicesEndpoint { get; set; }
    [Required]
    public string AzureAiServicesKey { get; set; }
    [Required]
    public string AzureOpenAiEndpoint { get; set; }
    [Required]
    public string AzureOpenAiKey { get; set; }
    [Required]
    public string OpenAiDeploymentName { get; set; }

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
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
