using Azure.AI.OpenAI;
using BatComputer.Plugins.Vision;
using BatComputer.Plugins.Weather.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.Core;
using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.Abstractions.Widgets;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using ChatHistory = Microsoft.SemanticKernel.AI.ChatCompletion.ChatHistory;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.SemanticKernel.Events;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web;
using Microsoft.SemanticKernel.Orchestration;

namespace MattEland.BatComputer.Kernel;

public class AppKernel : IAppKernel
{
    private readonly KernelSettings _settings;
    public IKernel Kernel { get; }
    public SequentialPlanner? Planner { get; }
    private readonly SequentialPlannerConfig _plannerConfig;
    private readonly ChatPlugin _chat;

    public Plan? LastPlan { get; private set; }

    public AppKernel(KernelSettings settings)
    {
        _settings = settings;
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
        Kernel.ImportFunctions(new CameraPlugin(this), "Vision");
        Kernel.ImportFunctions(new MePlugin(_settings, this), "User");
        Kernel.ImportFunctions(new ConversationSummaryPlugin(Kernel), "Summary");

        if (!string.IsNullOrWhiteSpace(_settings.BingKey))
        {
            WebSearchConnector = new BingConnector(_settings.BingKey);
            Kernel.ImportFunctions(new WebSearchEnginePlugin(WebSearchConnector), "Search");
        }

        _plannerConfig = new SequentialPlannerConfig();
        _plannerConfig.AllowMissingFunctions = false;
        _plannerConfig.ExcludedPlugins.Add("ConversationSummaryPlugin");
        _plannerConfig.ExcludedFunctions.Add("GetConversationTopics");
        _plannerConfig.ExcludedFunctions.Add("GetConversationActionItems");

        Planner = new SequentialPlanner(Kernel, _plannerConfig);
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
            AddWidget(new TokenUsageWidget(usage.PromptTokens, usage.CompletionTokens));
        }
    }

    public BingConnector WebSearchConnector { get; }

    public bool IsFunctionExcluded(FunctionView f) 
        => f.PluginName == "SequentialPlanner_Excluded" ||
            _plannerConfig.ExcludedFunctions.Contains(f.Name) ||
            _plannerConfig.ExcludedPlugins.Contains(f.PluginName);
    
    public bool HasPlanner => Planner != null;
    public string? LastMessage { get; private set; }
    public string? LastGoal { get; private set; }
    public Queue<IWidget> Widgets { get; } = new();
    public string SystemText { get; set; } = "You are an AI assistant named Alfred, the virtual butler to Batman. The user is Batman.";

    public void AddWidget(IWidget widget)
    {
        Widgets.Enqueue(widget);
    }

    public async Task<Plan> PlanAsync(string userText)
    {
        if (!HasPlanner)
        {
            throw new InvalidOperationException("The planner is not enabled");
        }

        LastPlan = null;
        LastMessage = null;
        Widgets.Clear();

        LastGoal = $"The goal of the plan is to answer the prompt: {userText} in the voice of Alfred addressing the user, Batman. Do not use .output in your plans and do not include any step that has no output.";
        Plan plan = await Planner!.CreatePlanAsync(LastGoal);

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
            LastGoal = null;
            Widgets.Clear();

            return await _chat.GetChatResponse(prompt);
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
