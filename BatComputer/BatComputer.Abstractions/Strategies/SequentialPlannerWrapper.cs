﻿using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;

namespace MattEland.BatComputer.Abstractions.Strategies;

public class SequentialPlannerWrapper : Planner
{
    private readonly SequentialPlanner _planner;

    public SequentialPlannerWrapper(SequentialPlanner planner) 
    {
        _planner = planner;
    }

    public override async Task<PlanWrapper> CreatePlanAsync(string goal)
        => new PlanWrapper(await _planner.CreatePlanAsync(goal));
}
