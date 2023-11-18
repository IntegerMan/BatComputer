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
using ChatHistory = Microsoft.SemanticKernel.AI.ChatCompletion.ChatHistory;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.SemanticKernel.Events;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Orchestration;
using MattEland.BatComputer.Abstractions.Strategies;

namespace MattEland.BatComputer.Kernel;

public class AppKernel : IAppKernel
{
    public IKernel Kernel { get; }
    public Planner? Planner { get; }
    private readonly ChatPlugin _chat;

    public Plan? LastPlan { get; private set; }

    public AppKernel(KernelSettings settings, PlannerStrategy? plannerStrategy)
    {
        KernelBuilder builder = new();
        builder.WithAzureOpenAIChatCompletionService(settings.OpenAiDeploymentName, settings.AzureOpenAiEndpoint, settings.AzureOpenAiKey);

        Kernel = builder.Build();

        Kernel.FunctionInvoking += OnFunctionInvoking;
        Kernel.FunctionInvoked += OnFunctionInvoked;

        _chat = new ChatPlugin(this);
        Kernel.ImportFunctions(_chat, "Chat");

        Kernel.ImportFunctions(new TimeContextPlugins(), "Time"); // NOTE: There's another more comprehensive time plugin
        Kernel.ImportFunctions(new WeatherPlugin(this), "Weather");
        Kernel.ImportFunctions(new LatLongPlugin(this), "LatLong");
        Kernel.ImportFunctions(new MePlugin(settings, this), "User");
        // Kernel.ImportFunctions(new ConversationSummaryPlugin(Kernel), "Summary");
        Kernel.ImportFunctions(new CameraPlugin(this), "Camera");

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
        if (!e.Metadata.TryGetValue("ModelResults", out object? value)) return;

        IReadOnlyCollection<ModelResult>? modelResults = value as IReadOnlyCollection<ModelResult>;
        CompletionsUsage? usage = modelResults?.First().GetOpenAIChatResult().Usage;

        if (usage is {TotalTokens: > 0})
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
    
    public bool HasPlanner => Planner != null;
    public string? LastMessage { get; private set; }
    public string? LastGoal { get; private set; }
    public PlanExecutionResult? LastResult { get; set; }

    public Queue<IWidget> Widgets { get; } = new();
    public string SystemText { get; set; } = "You are an AI assistant named Alfred, the virtual butler to Batman. The user is Batman.";

    public void AddWidget(IWidget widget) => Widgets.Enqueue(widget);

    public async Task<Plan> PlanAsync(string userText)
    {
        if (!HasPlanner)
        {
            throw new InvalidOperationException("The planner is not enabled");
        }

        LastPlan = null;
        LastMessage = null;
        LastResult = null;
        LastGoal = userText;

        Widgets.Clear();

        Plan plan = await Planner!.CreatePlanAsync(userText);

        LastPlan = plan;
        LastMessage = userText;

        return plan;
    }

    public async Task<string> GetChatPromptResponseAsync(string prompt)
    {
        try
        {
            LastPlan = null;
            LastMessage = prompt;
            LastResult = null;
            LastGoal = null;
            Widgets.Clear();

            string response = await _chat.GetChatResponse(prompt);

            LastResult = new PlanExecutionResult()
            {
                Output = response,
            };

            return response;
        }
        catch (HttpOperationException ex)
        {
            if (ex.Message.Contains("does not work with the specified model"))
            {
                return "Your model does not support the current option. You may be trying to use a completion model with a chat feature or vice versa. Try using a different deployment model.";
            }
            throw;
        }
    }

    internal async Task<string> GetPromptedReplyAsync(string command)
    {
        IChatCompletion completion = Kernel.GetService<IChatCompletion>();
        ChatHistory chat = completion.CreateNewChat(command);
        IReadOnlyList<IChatResult> result = await completion.GetChatCompletionsAsync(chat);
        ChatMessageBase chatResult = await result[0].GetChatMessageAsync();

        return chatResult.Content;
    }
}
