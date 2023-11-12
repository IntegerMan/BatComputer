using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Plugins.Core;

namespace MattEland.BatComputer.Kernel;

public class BatKernel
{
    public IKernel Kernel { get; }
    public SequentialPlanner Planner { get; }

    public BatKernel(BatComputerSettings settings, ILoggerFactory loggerFactory)
    {
        KernelBuilder builder = new();
        Kernel = builder
            .WithLoggerFactory(loggerFactory)
            .WithAzureOpenAIChatCompletionService(settings.OpenAiDeploymentName,
                                                  settings.AzureOpenAiEndpoint,
                                                  settings.AzureOpenAiKey,
                                                  setAsDefault: true)
            /*
            .WithAzureOpenAIImageGenerationService(settings.AzureOpenAiEndpoint, 
                                                   settings.AzureOpenAiKey)
            */
            .Build();

        ImportFunctions();


        Planner = CreatePlanner();

        Planner.WithInstrumentation(loggerFactory);
    }

    private void ImportFunctions()
    {
        Kernel.ImportFunctions(new TimePlugin(), "Time");
        Kernel.ImportFunctions(new MathPlugin(), "Math");
        Kernel.ImportFunctions(new TextPlugin(), "Strings");
        Kernel.ImportFunctions(new ChatPlugin(Kernel), "Chat");
        Kernel.ImportFunctions(new ConversationSummaryPlugin(Kernel), "Summary");

        // TODO: Add a memory plugin
    }

    private readonly SequentialPlannerConfig _plannerConfig = new();

    private SequentialPlanner CreatePlanner()
    {
        _plannerConfig.AllowMissingFunctions = false;
        _plannerConfig.ExcludedPlugins.Add("SemanticFunctions");
        _plannerConfig.ExcludedPlugins.Add("ConversationSummaryPlugin");

        return new SequentialPlanner(Kernel, _plannerConfig);
    }

    public bool IsFunctionExcluded(FunctionView f) 
        => f.PluginName == "SequentialPlanner_Excluded" ||
            _plannerConfig.ExcludedFunctions.Contains(f.Name) ||
            _plannerConfig.ExcludedPlugins.Contains(f.PluginName);

    public BatKernel(BatComputerSettings settings)
    {
        KernelBuilder builder = new();
        Kernel = builder
            .WithAzureOpenAIChatCompletionService(settings.OpenAiDeploymentName,
                                                  settings.AzureOpenAiEndpoint,
                                                  settings.AzureOpenAiKey,
                                                  setAsDefault: true)
            /*
            .WithAzureOpenAIImageGenerationService(settings.AzureOpenAiEndpoint, 
                                                   settings.AzureOpenAiKey)
            */
            .Build();

        ImportFunctions();

        Planner = CreatePlanner();
    }
}
