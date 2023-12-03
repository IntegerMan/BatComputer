using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.Abstractions.Strategies;
using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.Kernel.FileMemoryStore;
using MattEland.BatComputer.Kernel.Plugins;
using MattEland.BatComputer.Plugins.Sessionize;
using MattEland.BatComputer.Plugins.Vision;
using MattEland.BatComputer.Plugins.Weather.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.Memory;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using System.Text.Json;

namespace MattEland.BatComputer.Kernel;

public class AppKernel : IAppKernel, IDisposable
{
    private readonly ISKFunction _chat;
    private Planner? _planner;
    private readonly BatComputerLoggerFactory _loggerFactory;

    public IKernel Kernel { get; }

    public Plan? LastPlan { get; private set; }

    public ISemanticTextMemory? Memory { get; private set; }

    public AppKernel(KernelSettings settings, PlannerStrategy? plannerStrategy)
    {
        // This logger helps get accurate events from planners as not all planners tell the kernel when they incur token costs
        _loggerFactory = new BatComputerLoggerFactory(this);

        // Build and configure the kernel
        // TODO: Widget logic can probably move into an attached service
        Kernel = new KernelBuilder()
            .WithLoggerFactory(_loggerFactory)
            .WithAzureOpenAIChatCompletionService(settings.OpenAiDeploymentName, settings.AzureOpenAiEndpoint, settings.AzureOpenAiKey)
            .WithAzureOpenAITextEmbeddingGenerationService(settings.EmbeddingDeploymentName!, settings.AzureOpenAiEndpoint, settings.AzureOpenAiKey) // TODO: Local embedding would be better
            .Build();

        // Semantic Kernel doesn't have a good common abstraction around its planners, so I'm using an abstraction layer around the various planners
        _planner = plannerStrategy?.BuildPlanner(Kernel);

        // Chat plugin is core and should always be available
        Kernel.ImportFunctions(new ChatPlugin(), "Chat");
        _chat = Kernel.Functions.GetFunction("Chat", nameof(ChatPlugin.GetChatResponse));

        // Memory is important for providing additional context
        if (settings.SupportsMemory)
        {
            MemoryCollectionName = settings.EmbeddingCollectionName;
            MemoryStore = new FileBackedMemory("MemoryStore.json");
            Memory = new MemoryBuilder()
                .WithLoggerFactory(_loggerFactory)
                .WithAzureOpenAITextEmbeddingGenerationService(settings.EmbeddingDeploymentName!, settings.AzureOpenAiEndpoint, settings.AzureOpenAiKey) // TODO: Local embedding would be better
                .WithMemoryStore(MemoryStore)
                .Build();
            Kernel.ImportFunctions(new TextMemoryPlugin(Memory), "Memory");
        }

        // TODO: Ultimately detection of plugins should come from outside of the app, aside from the chat plugin
        Kernel.ImportFunctions(new TimeContextPlugins(), "Time");
        Kernel.ImportFunctions(new WeatherPlugin(this), "Weather");
        Kernel.ImportFunctions(new LatLongPlugin(this), "LatLong");
        Kernel.ImportFunctions(new MePlugin(settings, this), "User");
        // Kernel.ImportFunctions(new CameraPlugin(this), "Camera"); // Works, but its presence flags content filtering on sexual content

        if (settings.SupportsAiServices)
        {
            Kernel.ImportFunctions(new VisionPlugin(this, settings.AzureAiServicesEndpoint, settings.AzureAiServicesKey), "Vision");
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

    public string? LastMessage { get; private set; }
    public string? LastGoal { get; private set; }
    public PlanExecutionResult? LastResult { get; set; }

    public Queue<IWidget> Widgets { get; } = new();
    public string SystemText { get; set; } = "You are an AI assistant named Alfred, the virtual butler to Batman. The user is Batman.";
    public string? MemoryCollectionName { get; } = "BatComputer";
    public IMemoryStore? MemoryStore { get; }

    public void AddWidget(IWidget widget) => Widgets.Enqueue(widget);

    public async Task<Plan> PlanAsync(string userText)
    {
        LastPlan = null;
        LastMessage = userText;
        LastResult = null;
        LastGoal = userText;
        Widgets.Clear();
        _tokenUsage.Clear();

        Plan plan = _planner is null
            ? new Plan(_chat)
            : await _planner.CreatePlanAsync(userText);

        // Ensure the log has fully updated
        _loggerFactory.Flush();

        LastPlan = plan;
        return plan;
    }

    public async Task<PlanExecutionResult> ExecutePlanAsync()
    {
        if (LastPlan == null)
        {
            throw new InvalidOperationException("No plan has been generated. Generate a plan first.");
        }

        PlanExecutionResult? executionResult = null;
        try
        {
            FunctionResult result = await LastPlan.InvokeAsync(Kernel);
            executionResult = result.ToExecutionResult(LastPlan);
        }
        catch (HttpOperationException ex)
        {
            executionResult = new PlanExecutionResult()
            {
                StepsCount = LastPlan.Steps.Count,
                FunctionsUsed = string.Join(", ", LastPlan.Steps.Select(s => s.Name)),
                Iterations = 1,
                Summary = new List<StepSummary>(0),
                Output = HandleContentModerationResult(ex.ResponseContent) ?? ex.Message
            };
        }

        // Ensure the log has fully updated
        _loggerFactory.Flush();

        AddWidget(new TokenUsageWidget(_tokenUsage));

        LastResult = executionResult;

        return executionResult ?? new PlanExecutionResult() { Output = "An unknown error occurred" };
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

    public void Dispose() => ((IDisposable)_loggerFactory).Dispose();

    public void ReportTokenUsage(int promptTokens, int completionTokens)
    {
        _tokenUsage.Add(new TokenUsage(promptTokens, TokenUsageType.Prompt));
        _tokenUsage.Add(new TokenUsage(completionTokens, TokenUsageType.Completion));
    }

    private readonly List<TokenUsage> _tokenUsage = new();
}