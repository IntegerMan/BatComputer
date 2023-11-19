using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.Plugins.Camera;
using MattEland.BatComputer.Plugins.Sessionize;
using MattEland.BatComputer.Plugins.Vision;
using MattEland.BatComputer.Plugins.Weather.Plugins;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Orchestration;
using MattEland.BatComputer.Abstractions.Strategies;

namespace MattEland.BatComputer.Kernel;

public class AppKernel : IAppKernel, IDisposable
{
    private readonly ISKFunction _chat;
    private Planner? _planner;
    private readonly BatComputerLoggerFactory _loggerFactory;

    public IKernel Kernel { get; }

    public Plan? LastPlan { get; private set; }

    public AppKernel(KernelSettings settings, PlannerStrategy? plannerStrategy)
    {
        _loggerFactory = new BatComputerLoggerFactory(this);

        Kernel = new KernelBuilder()
            .WithLoggerFactory(_loggerFactory)
            .WithAzureOpenAIChatCompletionService(settings.OpenAiDeploymentName, settings.AzureOpenAiEndpoint, settings.AzureOpenAiKey)
            /*
            .WithAIService<IChatCompletion>(serviceId: null, 
                    instance: new AzureOpenAIChatCompletion(settings.OpenAiDeploymentName, settings.AzureOpenAiEndpoint, settings.AzureOpenAiKey, 
                    loggerFactory: loggerFactory))
            */
            .Build();

        Kernel.ImportFunctions(new TimeContextPlugins(), "Time");
        Kernel.ImportFunctions(new WeatherPlugin(this), "Weather");
        Kernel.ImportFunctions(new LatLongPlugin(this), "LatLong");
        Kernel.ImportFunctions(new MePlugin(settings, this), "User");
        Kernel.ImportFunctions(new CameraPlugin(this), "Camera");
        Kernel.ImportFunctions(new ChatPlugin(this), "Chat");
        _chat = Kernel.Functions.GetFunction("Chat", nameof(ChatPlugin.GetChatResponse));

        if (settings.SupportsAiServices)
        {
            Kernel.ImportFunctions(new VisionPlugin(this, settings.AzureAiServicesEndpoint, settings.AzureAiServicesKey), "Vision");
        }

        if (settings.SupportsSearch)
        {
            WebSearchConnector = new BingConnector(settings.BingKey!);
            Kernel.ImportFunctions(new WebSearchEnginePlugin(WebSearchConnector), "Search");
        }

        if (settings.SupportsSessionize)
        {
            Kernel.ImportFunctions(new SessionizePlugin(this, settings.SessionizeToken!), "Sessionize");
        }

        _planner = plannerStrategy?.BuildPlanner(Kernel);
    }

    public void SwitchPlanner(PlannerStrategy? plannerStrategy)
    {
        _planner = plannerStrategy?.BuildPlanner(Kernel);
    }

    public BingConnector? WebSearchConnector { get; }

    public bool IsFunctionExcluded(FunctionView f)
        => f.PluginName.Contains("_Excluded", StringComparison.OrdinalIgnoreCase);

    public string? LastMessage { get; private set; }
    public string? LastGoal { get; private set; }
    public PlanExecutionResult? LastResult { get; set; }

    public Queue<IWidget> Widgets { get; } = new();
    public string SystemText { get; set; } = "You are an AI assistant named Alfred, the virtual butler to Batman. The user is Batman.";

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

        FunctionResult result = await LastPlan.InvokeAsync(Kernel);
        PlanExecutionResult executionResult = result.ToExecutionResult(LastPlan);

        // Ensure the log has fully updated
        _loggerFactory.Flush();

        AddWidget(new TokenUsageWidget(_tokenUsage));

        LastResult = executionResult;
        return executionResult;
    }

    public void Dispose() => ((IDisposable)_loggerFactory).Dispose();

    public void ReportTokenUsage(int promptTokens, int completionTokens)
    {
        _tokenUsage.Add(new TokenUsage(promptTokens, TokenUsageType.Prompt));
        _tokenUsage.Add(new TokenUsage(completionTokens, TokenUsageType.Completion));
    }

    private readonly List<TokenUsage> _tokenUsage = new();
}