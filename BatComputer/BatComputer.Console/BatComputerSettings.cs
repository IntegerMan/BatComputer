using System.ComponentModel.DataAnnotations;

namespace MattEland.BatComputer.ConsoleApp;

public class BatComputerSettings : IValidatableObject {
    [Required]
    public string AzureAiServicesEndpoint { get; set; }
    [Required]
    public string AzureAiServicesKey { get; internal set; }

    public IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> Validate(ValidationContext validationContext) {
        List<System.ComponentModel.DataAnnotations.ValidationResult> results = new();

        Validator.TryValidateObject(this, validationContext, results);

        return results;
    }

    public bool IsValid => !Validate(new ValidationContext(this)).Any();
}
