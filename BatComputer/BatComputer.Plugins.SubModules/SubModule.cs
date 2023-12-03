using MattEland.BatComputer.Abstractions.Strategies;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using System.ComponentModel;
using System.Reflection;

namespace BatComputer.Plugins.SubModules;

public class SubModule
{
    public string Name { get; set; }
    public string PluginName => $"{Name}Module";
    public string Description { get; set; }

    public SubModule(string name, string description, IPlannerProvider plannerProvider, IKernel kernel)
    {
        Name = name;
        Description = description;
        _planner = plannerProvider;
        _kernel = kernel;
    }

    private readonly List<FunctionView> _functions = new();
    private readonly IPlannerProvider _planner;
    private readonly IKernel _kernel;

    public ILoggerFactory? LoggerFactory { get; set; }
    public Plan? LastPlan { get; private set; }
    public FunctionResult? LastResult { get; private set; }

    [SKFunction] // TODO: It'd be nice to add a description using the Description property
    public async Task<string> InvokeAsync(string input)
    {
        if (_functions.Count == 0)
        {
            return $"No functions have been added to {PluginName}";
        }

        // TODO: hand this off to a separate planner / kernel
        ILogger<SubModule>? logger = LoggerFactory?.CreateLogger<SubModule>();
        logger?.LogInformation($"{PluginName} invoked with {input}");

        Plan plan = await _planner.CreatePlanAsync(input, _kernel, _functions);
        FunctionResult funcResult = await plan.InvokeAsync(input, _kernel);

        string? output = funcResult.GetValue<string>();
        logger?.LogInformation($"{PluginName} resulted in {output}");

        LastPlan = plan;
        LastResult = funcResult;

        return output ?? $"{PluginName} did not return any result";
    }

    public void Add(object function, string? name = null, string? description = null)
    {
        // Code based on code from Semantic Kernel's Kernel.ImportFunctions
        MethodInfo[] methods = function.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public);

        foreach (MethodInfo methodInfo in methods)
        {
            if (methodInfo.GetCustomAttribute<SKFunctionAttribute>() == null)
            {
                continue;
            }

            ISKFunction skFunc = SKFunction.Create(methodInfo, function, pluginName: PluginName, functionName: name, description: description, null, null, LoggerFactory);
            _kernel.RegisterCustomFunction(skFunc);

            _functions.Add(skFunc.Describe());
        }
    }

    public override string ToString() => PluginName;
}
