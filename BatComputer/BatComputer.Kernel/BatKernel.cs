using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Planners;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Plugins.Core;

namespace MattEland.BatComputer.Kernel;

public class BatKernel
{
    public IKernel Kernel { get; }
    public SequentialPlanner Planner { get; }
    private readonly ChatPlugin _chat;

    public BatKernel(BatComputerSettings settings, ILoggerFactory loggerFactory)
    {
        KernelBuilder builder = new();
        builder.WithLoggerFactory(loggerFactory);
        Kernel = BuildKernel(settings, builder);

        _chat = new ChatPlugin(Kernel);

        ImportFunctions();

        Planner = CreatePlanner();
        Planner.WithInstrumentation(loggerFactory);
    }

    public BatKernel(BatComputerSettings settings)
    {
        KernelBuilder builder = new();
        Kernel = BuildKernel(settings, builder);

        _chat = new ChatPlugin(Kernel);

        ImportFunctions();

        Planner = CreatePlanner();
    }
    private static IKernel BuildKernel(BatComputerSettings settings, KernelBuilder builder)
    {
        return builder
            .WithAzureOpenAIChatCompletionService(settings.OpenAiDeploymentName,
                                                  settings.AzureOpenAiEndpoint,
                                                  settings.AzureOpenAiKey,
                                                  setAsDefault: true)
            // TODO: Add ImageGen Service
            // TODO: Add Embeddings
            .Build();
    }

    private void ImportFunctions()
    {
        Kernel.ImportFunctions(_chat, "Chat");
        Kernel.ImportFunctions(new TimeContextPlugins(), "Time"); // NOTE: There's another more comprehensive time plugin
        Kernel.ImportFunctions(new MathPlugin(), "Math");
        Kernel.ImportFunctions(new TextPlugin(), "Strings");
        Kernel.ImportFunctions(new HttpPlugin(), "HTTP");
        Kernel.ImportFunctions(new ConversationSummaryPlugin(Kernel), "Summary");

        // TODO: Add a memory plugin
    }

    private readonly SequentialPlannerConfig _plannerConfig = new();

    private SequentialPlanner CreatePlanner()
    {
        _plannerConfig.AllowMissingFunctions = false;
        _plannerConfig.ExcludedPlugins.Add("SemanticFunctions");
        _plannerConfig.ExcludedPlugins.Add("ConversationSummaryPlugin");

        return new SequentialPlanner(Kernel, _plannerConfig);
    }

    public bool IsFunctionExcluded(FunctionView f) 
        => f.PluginName == "SequentialPlanner_Excluded" ||
            _plannerConfig.ExcludedFunctions.Contains(f.Name) ||
            _plannerConfig.ExcludedPlugins.Contains(f.PluginName);

    public async Task<string> GetChatResponseAsync(string prompt)
    {
        return await _chat.GetChatResponse(prompt);
    }

    public async Task<Plan> PlanAsync(string userText)
    {
        string goal = $"User: {userText}" + """

                                            ---------------------------------------------

                                            Respond to this statement. If the user is requesting information about a web page by URL, make a GET request for that data and summarize the contents.
                                            """;
        return await Planner.CreatePlanAsync(goal);
    }
}
