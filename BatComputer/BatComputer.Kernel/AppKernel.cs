using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.Plugins.Camera;
using MattEland.BatComputer.Plugins.Sessionize;
using MattEland.BatComputer.Plugins.Vision;
using MattEland.BatComputer.Plugins.Weather.Plugins;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Events;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Orchestration;
using MattEland.BatComputer.Abstractions.Strategies;
using ChatMessage = Microsoft.SemanticKernel.AI.ChatCompletion.ChatMessage;

namespace MattEland.BatComputer.Kernel;

public class AppKernel : IAppKernel
{
    private readonly ISKFunction _chat;

    public IKernel Kernel { get; }
    public Planner? Planner { get; }

    public Plan? LastPlan { get; private set; }

    public AppKernel(KernelSettings settings, PlannerStrategy? plannerStrategy)
    {
        KernelBuilder builder = new();
        builder.WithAzureOpenAIChatCompletionService(settings.OpenAiDeploymentName, settings.AzureOpenAiEndpoint, settings.AzureOpenAiKey);

        Kernel = builder.Build();

        Kernel.FunctionInvoking += OnFunctionInvoking;
        Kernel.FunctionInvoked += OnFunctionInvoked;

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

        IEnumerable<string> excludedPlugins = [];
        IEnumerable<string> excludedFunctions = [];

        Planner = plannerStrategy?.BuildPlanner(Kernel, excludedPlugins, excludedFunctions);
    }

    private void OnFunctionInvoking(object? sender, FunctionInvokingEventArgs e)
    {
    }

    private void OnFunctionInvoked(object? sender, FunctionInvokedEventArgs e)
    {
        if (!e.Metadata.TryGetValue("ModelResults", out object? value))
            return;

        IReadOnlyCollection<ModelResult>? modelResults = value as IReadOnlyCollection<ModelResult>;
        CompletionsUsage? usage = modelResults?.First().GetOpenAIChatResult().Usage;

        if (usage is { TotalTokens: > 0 })
        {
            string stepName = e.FunctionView.Name.StartsWith("func", StringComparison.OrdinalIgnoreCase)
                    ? e.FunctionView.PluginName.Replace("_Excluded", "", StringComparison.OrdinalIgnoreCase)
                    : e.FunctionView.Name;

            AddWidget(new TokenUsageWidget(usage.PromptTokens, usage.CompletionTokens, $"{stepName} Token Usage"));
        }
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

        Plan plan = Planner is null 
            ? new Plan(_chat) 
            : await Planner.CreatePlanAsync(userText);

        LastPlan = plan;
        return plan;
    }

    internal async Task<string> GetPromptedReplyAsync(string command)
    {
        IChatCompletion completion = Kernel.GetService<IChatCompletion>();
        ChatHistory chat = completion.CreateNewChat(command);
        IReadOnlyList<IChatResult> result = await completion.GetChatCompletionsAsync(chat);
        ChatMessage chatResult = await result[0].GetChatMessageAsync();

        return chatResult.Content;
    }

    public async Task<PlanExecutionResult> ExecutePlanAsync()
    {
        if (LastPlan == null)
        {
            throw new InvalidOperationException("No plan has been generated. Generate a plan first.");
        }

        FunctionResult result = await LastPlan.InvokeAsync(Kernel);
        PlanExecutionResult executionResult = result.ToExecutionResult(LastPlan);

        LastResult = executionResult;
        return executionResult;
    }
}