using BatComputer.Plugins.Weather;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.Core;
using LLamaSharp.SemanticKernel.TextCompletion;
using Microsoft.SemanticKernel.AI.TextCompletion;
using LLama.Common;
using LLama;

namespace MattEland.BatComputer.Kernel;

public class AppKernel
{
    private readonly KernelSettings _settings;
    public IKernel Kernel { get; }
    public SequentialPlanner? Planner { get; }
    private readonly ChatPlugin _chat;

    public Plan? LastPlan { get; private set; }

    public AppKernel(KernelSettings settings, ILoggerFactory loggerFactory)
    {
        _settings = settings;
        KernelBuilder builder = new();
        builder.WithLoggerFactory(loggerFactory);
        Kernel = BuildKernel(settings, builder);

        _chat = new ChatPlugin(Kernel);

        ImportFunctions();

        Planner = CreatePlanner();
        Planner.WithInstrumentation(loggerFactory);
    }

    public AppKernel(KernelSettings settings)
    {
        _settings = settings;
        KernelBuilder builder = new();
        Kernel = BuildKernel(settings, builder);

        _chat = new ChatPlugin(Kernel);

        ImportFunctions();

        Planner = CreatePlanner();
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
        Kernel.ImportFunctions(new MePlugin(_settings, Kernel), "User");
        Kernel.ImportFunctions(new ConversationSummaryPlugin(Kernel), "Summary");

        // TODO: Add a memory plugin
    }

    private readonly SequentialPlannerConfig _plannerConfig = new();
    private static LLamaWeights _model;
    private static LLamaContext _context;

    private SequentialPlanner CreatePlanner()
    {
        _plannerConfig.AllowMissingFunctions = false;
        _plannerConfig.ExcludedPlugins.Add("SemanticFunctions");
        _plannerConfig.ExcludedPlugins.Add("ConversationSummaryPlugin");
        _plannerConfig.ExcludedFunctions.Add("GetConversationTopics");
        _plannerConfig.ExcludedFunctions.Add("GetConversationActionItems");

        return new SequentialPlanner(Kernel, _plannerConfig);
    }

    public bool IsFunctionExcluded(FunctionView f) 
        => f.PluginName == "SequentialPlanner_Excluded" ||
            _plannerConfig.ExcludedFunctions.Contains(f.Name) ||
            _plannerConfig.ExcludedPlugins.Contains(f.PluginName);

    public async Task<string> GetChatResponseAsync(string prompt)
    {
        LastPlan = null;

        return await _chat.GetChatResponse(prompt);
    }

    public bool HasPlanner => Planner != null;

    public async Task<Plan> PlanAsync(string userText)
    {
        if (!HasPlanner)
        {
            throw new InvalidOperationException("The planner is not enabled");
        }

        LastPlan = null;

        string goal = $"User: {userText}" + """

                                            ---------------------------------------------

                                            Respond to this statement as if you are Alfred and the user is Batman.
                                            """;
        Plan plan = await Planner!.CreatePlanAsync(goal);

        LastPlan = plan;

        return plan;
    }
}
