using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;

namespace MattEland.BatComputer.Abstractions.Strategies;

public interface IPlannerProvider
{
    Task<Plan> CreatePlanAsync(string goal, IKernel kernel, IEnumerable<FunctionView> functions);
}