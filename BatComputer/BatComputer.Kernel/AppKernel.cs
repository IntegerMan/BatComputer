using BatComputer.Plugins.Weather;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.Core;
using LLamaSharp.SemanticKernel.TextCompletion;
using Microsoft.SemanticKernel.AI.TextCompletion;
using LLama.Common;
using LLama;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using ChatHistory = Microsoft.SemanticKernel.AI.ChatCompletion.ChatHistory;

namespace MattEland.BatComputer.Kernel;

public class AppKernel
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
        Kernel.ImportFunctions(new OpenMeteoPlugin(), "Weather");
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

    public async Task<string> GetChatResponseAsync(string prompt)
    {
        LastPlan = null;
        LastMessage = prompt;
        LastGoal = null;

        return await _chat.GetChatResponse(prompt);
    }

    public bool HasPlanner => Planner != null;
    public string? LastMessage { get; private set; }
    public string? LastGoal { get; private set; }

    public async Task<Plan> PlanAsync(string userText)
    {
        if (!HasPlanner)
        {
            throw new InvalidOperationException("The planner is not enabled");
        }

        LastPlan = null;
        LastMessage = null;

        LastGoal = """
                      The following is a chat transcript between the user and you. Respond in the best way that you can, retrieving the most relevant information possible.
                      ---------------------------------------------
                      """ +
                      $"User: {userText}\r\n" + 
                      "Bot: ";
        Plan plan = await Planner!.CreatePlanAsync(LastGoal);

        LastPlan = plan;
        LastMessage = userText;

        return plan;
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
