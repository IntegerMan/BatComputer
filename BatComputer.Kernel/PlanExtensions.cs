using Microsoft.SemanticKernel.Planning;

namespace MattEland.BatComputer.Kernel;

public static class PlanExtensions
{
    public static string GetTarget(this Plan plan)
    {
        string targetKey;
        if (plan.Outputs.Count == 0)
        {
            // We shouldn't have had a plan with no outputs, but this is a good key to use just in case
            targetKey = "RESULT__RESPONSE";
        }
        else
        {
            // Either we have 1 or more than 1 output variable. In either case, let's go with the last entry as the most likely usable one
            targetKey = plan.Outputs.Last();
        }

        return targetKey;
    }
}
