using System.ComponentModel.DataAnnotations;

namespace MattEland.BatComputer.ConsoleApp;

public class BatComputerSettings {
    [Required]
    public string AzureAiServicesEndpoint { get; set; }
    [Required]
    public string AzureAiServicesKey { get; set; }
    [Required]
    public string AzureOpenAiEndpoint { get; set; }
    [Required]
    public string AzureOpenAiKey { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
        List<ValidationResult> results = new();

        Validator.TryValidateObject(this, validationContext, results);

        return results;
    }

    public bool IsValid => !Validate(new ValidationContext(this)).Any();
}
