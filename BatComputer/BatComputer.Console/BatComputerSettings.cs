using System;
using System.ComponentModel.DataAnnotations;

namespace MattEland.BatComputer.ConsoleApp;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

public class BatComputerSettings {
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

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
        List<ValidationResult> results = new();

        Validator.TryValidateObject(this, validationContext, results);

        return results;
    }

    public bool IsValid => !Validate(new ValidationContext(this)).Any();
}
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
