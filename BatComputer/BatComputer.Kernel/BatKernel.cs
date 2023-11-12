using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;

namespace MattEland.BatComputer.Kernel;

public class BatKernel
{
    public IKernel Kernel { get; }
    public IActionPlanner Planner { get; }

    public BatKernel(BatComputerSettings settings, ILoggerFactory loggerFactory)
    {
        this.Kernel = BuildKernel(settings, loggerFactory);
        this.Planner = BuildPlanner(this.Kernel);
    }

    private static IKernel BuildKernel(BatComputerSettings settings, ILoggerFactory loggerFactory)
    {
        KernelBuilder builder = new();
        IKernel kernel = builder
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

        kernel.ImportFunctions(new TimeContextPlugins(), "TimeContext");

        return kernel;
    }

    private static IActionPlanner BuildPlanner(IKernel kernel)
    {
        ActionPlanner planner = new(kernel);

        return planner;
    }

}
