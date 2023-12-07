using MattEland.BatComputer.Abstractions.Strategies;
using MattEland.BatComputer.Kernel.ContentFiltering;
using MattEland.BatComputer.Kernel.FileMemoryStore;
using MattEland.BatComputer.Kernel.Plugins;
using MattEland.BatComputer.Plugins.Sessionize;
using MattEland.BatComputer.Plugins.Vision;
using MattEland.BatComputer.Plugins.Weather.Plugins;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using System.Text.Json;

namespace MattEland.BatComputer.Kernel;

public class AppKernel
{
    private Planner? _planner;

    public IKernel Kernel { get; }

    public Plan? LastPlan { get; private set; }

    public ISemanticTextMemory? Memory { get; private set; }

    public AppKernel(KernelSettings settings, PlannerStrategy? plannerStrategy, ILoggerFactory loggerFactory)
    {
        // TODO: Optionally use non-Azure chat completion
        IChatCompletion chatCompletion = new AzureOpenAIChatCompletion(
            settings.OpenAiDeploymentName, 
            settings.AzureOpenAiEndpoint, 
            settings.AzureOpenAiKey, 
            loggerFactory: loggerFactory);

        // Build and configure the kernel
        Kernel = new KernelBuilder()
            .WithLoggerFactory(loggerFactory)
            .WithAIService<IChatCompletion>(null, new VerboseLoggingChatCompletion(chatCompletion, loggerFactory))
            .WithAzureOpenAITextEmbeddingGenerationService(settings.EmbeddingDeploymentName!, settings.AzureOpenAiEndpoint, settings.AzureOpenAiKey) // TODO: Local embedding would be better
            .Build();

        // Semantic Kernel doesn't have a good common abstraction around its planners, so I'm using an abstraction layer around the various planners
        _planner = plannerStrategy?.BuildPlanner(Kernel);

        // Memory is important for providing additional context
        if (settings.SupportsMemory)
        {
            MemoryCollectionName = settings.EmbeddingCollectionName;
            MemoryStore = new FileBackedMemory("MemoryStore.json");
            Memory = new MemoryBuilder()
                .WithLoggerFactory(loggerFactory)
                .WithAzureOpenAITextEmbeddingGenerationService(settings.EmbeddingDeploymentName!, settings.AzureOpenAiEndpoint, settings.AzureOpenAiKey) // TODO: Local embedding would be better
                .WithMemoryStore(MemoryStore)
                .Build();
            // Kernel.ImportFunctions(new TextMemoryPlugin(Memory), "Memory");
        }

        // TODO: Ultimately detection of plugins should come from outside of the app, aside from the chat plugin
        Kernel.ImportFunctions(new TimeContextPlugins(), "Time");
        Kernel.ImportFunctions(new WeatherPlugin(), "Weather");
        Kernel.ImportFunctions(new LatLongPlugin(), "LatLong");
        Kernel.ImportFunctions(new MePlugin(), "User");
//        Kernel.ImportFunctions(new CameraPlugin(), "Camera"); // Works, but its presence flags content filtering on sexual content

        if (settings.SupportsAiServices)
        {
            Kernel.ImportFunctions(new VisionPlugin(settings.AzureAiServicesEndpoint, settings.AzureAiServicesKey), "Vision");
        }

        if (settings.SupportsSearch)
        {
            var searchConnector = new BingConnector(settings.BingKey!);
            Kernel.ImportFunctions(new WebSearchEnginePlugin(searchConnector), "Search");
        }

        if (settings.SupportsSessionize)
        {
            Kernel.ImportFunctions(new SessionizePlugin(Memory, settings.SessionizeToken!), "Sessionize");
        }
    }

    public void SwitchPlanner(PlannerStrategy? plannerStrategy)
    {
        _planner = plannerStrategy?.BuildPlanner(Kernel);
    }

    public bool IsFunctionExcluded(FunctionView f)
        => f.PluginName.Contains("_Excluded", StringComparison.OrdinalIgnoreCase);

    public PlanExecutionResult? LastResult { get; set; }

    public string? MemoryCollectionName { get; } = "BatComputer";
    public IMemoryStore? MemoryStore { get; }

    public async Task<string> ExecuteAsync(string userText)
    {
        LastPlan = null;
        LastResult = null;

        Plan plan = _planner is null
            ? new Plan(userText)
            : await _planner.CreatePlanAsync(userText);

        LastPlan = plan;

        string? output = null;

        PlanExecutionResult? executionResult = null;
        try
        {
            FunctionResult result = await plan.InvokeAsync(Kernel);
            executionResult = result.ToExecutionResult(plan);
        }
        catch (HttpOperationException ex)
        {
            executionResult = new PlanExecutionResult()
            {
                StepsCount = plan.Steps.Count,
                FunctionsUsed = string.Join(", ", LastPlan.Steps.Select(s => s.Name)),
                Iterations = 1,
                Summary = [],
                Output = HandleContentModerationResult(ex.ResponseContent) ?? ex.Message
            };
        }

        LastResult = executionResult;
        output = executionResult.Output;

        return output ?? "An unknown error occurred";
    }

    private static string? HandleContentModerationResult(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        ContentResponse? response = JsonSerializer.Deserialize<ContentResponse>(json);
        ContentFilterResult? filter = response?.error?.innererror?.content_filter_result;

        if (filter != null)
        {
            string disclaimer = " This can be fixed by adjusting your prompt or by relaxing content moderation settings in Azure.";
            if (filter.sexual.filtered)
            {
                return $"The request was flagged for {filter.sexual.severity} sexual content. {disclaimer}";
            }
            else if (filter.hate.filtered)
            {
                return $"The request was flagged for {filter.hate.severity} hate content. {disclaimer}";
            }
            else if (filter.self_harm.filtered)
            {
                return $"The request was flagged for {filter.self_harm.severity} self-harm content. {disclaimer}";
            }
            else if (filter.violence.filtered)
            {
                return $"The request was flagged for {filter.violence.severity} violent content. {disclaimer}";
            }
        }

        return null;
    }
}