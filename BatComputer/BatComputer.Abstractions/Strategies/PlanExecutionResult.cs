namespace MattEland.BatComputer.Abstractions.Strategies;

public class PlanExecutionResult
{
    public string? Output { get; set; }
    public int StepsCount { get; set; }
    public int Iterations { get; set; }
    public string? FunctionsUsed { get; set; }
    public List<StepSummary> Summary { get; set; } = new();
}
