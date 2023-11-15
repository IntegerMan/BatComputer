using BatComputer.Plugins.Weather.Plugins;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.Core;
using LLamaSharp.SemanticKernel.TextCompletion;
using Microsoft.SemanticKernel.AI.TextCompletion;
using LLama.Common;
using LLama;
using MattEland.BatComputer.Abstractions;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using ChatHistory = Microsoft.SemanticKernel.AI.ChatCompletion.ChatHistory;
using Microsoft.SemanticKernel.Diagnostics;

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
        Kernel = BuildKernel(settings, builder);

        _chat = new ChatPlugin(this);

        ImportFunctions();

        _plannerConfig = new SequentialPlannerConfig();
        _plannerConfig.AllowMissingFunctions = false;
        _plannerConfig.ExcludedPlugins.Add("ConversationSummaryPlugin");
        _plannerConfig.ExcludedFunctions.Add("GetConversationTopics");
        _plannerConfig.ExcludedFunctions.Add("GetConversationActionItems");

        Planner = new SequentialPlanner(Kernel, _plannerConfig);
    }

    private static IKernel BuildKernel(KernelSettings settings, KernelBuilder builder)
    {
        bool useLlama = false;

        if (useLlama)
        {
            var parameters = new ModelParams(@"C:\Models\thespis-13b-v0.5.Q2_K.gguf");
            _model = LLamaWeights.LoadFromFile(parameters);
            //var ex = new StatelessExecutor(model, parameters);

            _context = _model.CreateContext(parameters);
            // LLamaSharpChatCompletion requires InteractiveExecutor, as it's the best fit for the given command.
            //InteractiveExecutor chatEx = new(_context);
            StatelessExecutor stateEx = new(_model, parameters);

            builder.WithAIService<ITextCompletion>("local-llama", new LLamaSharpTextCompletion(stateEx), true);
            //builder.WithAIService<IChatCompletion>("local-llama-chat", new LLamaSharpChatCompletion(chatEx), setAsDefault: true);
        }
        else
        {
            /*
.WithAzureOpenAIChatCompletionService(settings.OpenAiDeploymentName,
                                      settings.AzureOpenAiEndpoint,
                                      settings.AzureOpenAiKey,
                                      setAsDefault: true)
*/
            // TODO: Allow specifying text vs chat models
            builder.WithAzureOpenAIChatCompletionService(settings.OpenAiDeploymentName, settings.AzureOpenAiEndpoint, settings.AzureOpenAiKey);
        }

        return builder

            // TODO: Add ImageGen Service
            // TODO: Add Embeddings
            // TODO: Add Retry & Polly
            .Build();
    }

    private void ImportFunctions()
    {
        Kernel.ImportFunctions(_chat, "Chat");
        Kernel.ImportFunctions(new TimeContextPlugins(), "Time"); // NOTE: There's another more comprehensive time plugin
        //Kernel.ImportFunctions(new MathPlugin(), "Math");
        //Kernel.ImportFunctions(new TextPlugin(), "Strings");
        //Kernel.ImportFunctions(new HttpPlugin(), "HTTP");
        Kernel.ImportFunctions(new WeatherPlugin(this), "Weather");
        Kernel.ImportFunctions(new LatLongPlugin(this), "LatLong");
        Kernel.ImportFunctions(new MePlugin(_settings, this), "User");
        Kernel.ImportFunctions(new ConversationSummaryPlugin(Kernel), "Summary");

        // TODO: Add a memory plugin
    }

    private static LLamaWeights _model;
    private static LLamaContext _context;

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
