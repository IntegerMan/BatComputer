﻿using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;

namespace MattEland.BatComputer.Abstractions.Strategies;

public class SequentialPlannerStrategy : PlannerStrategy
{
    public override Planner? BuildPlanner(IKernel kernel, 
        IEnumerable<string>? excludedPlugins, 
        IEnumerable<string>? excludedFunctions)
    {
        // Configure the planner
        SequentialPlannerConfig config = new();
        config.AllowMissingFunctions = false;
        // config.RelevancyThreshold = 0.6;

        ExcludeFunctions(config, excludedPlugins, excludedFunctions);

        // Create the planner
        SequentialPlanner planner = new(kernel, config);

        return new SequentialPlannerWrapper(planner);
    }

    public override string ToString() => "Sequential Planner";
}
