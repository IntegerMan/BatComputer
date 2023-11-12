using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Planners;

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

        Kernel.ImportFunctions(new Microsoft.SemanticKernel.Plugins.Core.TimePlugin(), "TimePlugin");
        Kernel.ImportFunctions(new ChatPlugin(Kernel), "ChatPlugin");

        Planner = CreatePlanner();

        Planner.WithInstrumentation(loggerFactory);
    }

    private SequentialPlanner CreatePlanner()
    {
        SequentialPlannerConfig config = new()
        {
            AllowMissingFunctions = false,
        };
        config.ExcludedFunctions.Add("Chat");
        return new SequentialPlanner(Kernel, config);
    }

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

        Kernel.ImportFunctions(new Microsoft.SemanticKernel.Plugins.Core.TimePlugin(), "TimePlugin");
        Kernel.ImportFunctions(new ChatPlugin(Kernel), "ChatPlugin");

        Planner = CreatePlanner();
    }
}
