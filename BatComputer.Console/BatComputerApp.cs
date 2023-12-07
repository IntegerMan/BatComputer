using MattEland.BatComputer.Kernel;
using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.Abstractions.Strategies;
using MattEland.BatComputer.ConsoleApp.Abstractions;
using MattEland.BatComputer.ConsoleApp.Commands;
using MattEland.BatComputer.ConsoleApp.Helpers;
using MattEland.BatComputer.ConsoleApp.Menus;
using MattEland.BatComputer.ConsoleApp.Renderables;
using MattEland.BatComputer.ConsoleApp.Skins;
using MattEland.BatComputer.Speech;
using MattEland.BatComputer.Abstractions;
using Spectre.Console;
using Microsoft.SemanticKernel.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI.ChatCompletion;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Planning;
using Microsoft.SemanticKernel.Orchestration;
using System.Text.Json;
using MattEland.BatComputer.Kernel.ContentFiltering;
using MattEland.BatComputer.Kernel.Plugins;
using MattEland.BatComputer.Plugins.Sessionize;
using MattEland.BatComputer.Plugins.Vision;
using MattEland.BatComputer.Plugins.Weather.Plugins;
using Microsoft.SemanticKernel.Plugins.Web.Bing;
using Microsoft.SemanticKernel.Plugins.Web;

namespace MattEland.BatComputer.ConsoleApp;

public class BatComputerApp : IDisposable
{
    public ConsoleSkin Skin { get; set; } = new BatComputerSkin();
    public IKernel? Kernel { get; private set; }

    public async Task<int> RunAsync()
    {
        // Fancy header
        WelcomeRenderer.ShowWelcome(Skin);

        // Handle app settings
        Settings.Load(Skin);
        Settings.Validate();
        if (!Settings.SupportsSearch)
        {
            new WarningWidget("No Bing Search Key Supplied. Web Search will be disabled.").Render(Skin);
        }
        if (!Settings.SupportsMemory)
        {
            new WarningWidget("No Embeddings Deployment Name or Embeddings Collection Name. Memory will be disabled.").Render(Skin);
        }

        // Warn the user that actual costs may be incurred from using the app
        if (!Settings.SkipCostDisclaimer)
        {
            new InfoWidget("Disclaimer", "This app uses chat, text completions, vision, speech, search, and other features that have costs associated with them. The developer will not be responsible for costs resulting from its usage.").Render(Skin);
            if (!AnsiConsole.Confirm("Do you agree to these terms and wish to continue?"))
            {
                AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]Program terminating[/]");
                return 0;
            }
        }

        // Greet the user and set up speech if it's configured
        if (Settings.SupportsAiServices)
        {
            Speech = new SpeechProvider(Settings.AzureAiServicesRegion, Settings.AzureAiServicesKey, Settings.SpeechVoiceName);
            Speech.EnableSpeech = Settings.IsSpeechEnabled;
            _ = Speech.SpeakAsync($"Welcome to {Skin.AppNameWithPrefix}");
        }

        // Configure the application
        ILoggerFactory loggerFactory = new BatComputerLoggerFactory(this);

        // Create the kernel
        IChatCompletion chatCompletion = new AzureOpenAIChatCompletion(
            Settings.OpenAiDeploymentName,
            Settings.AzureOpenAiEndpoint,
            Settings.AzureOpenAiKey,
            loggerFactory: loggerFactory);

        // Build and configure the kernel
        Kernel = new KernelBuilder()
            .WithLoggerFactory(loggerFactory)
            .WithAIService<IChatCompletion>(null, new VerboseLoggingChatCompletion(chatCompletion, loggerFactory))
            .WithAzureOpenAITextEmbeddingGenerationService(Settings.EmbeddingDeploymentName!, Settings.AzureOpenAiEndpoint, Settings.AzureOpenAiKey) // TODO: Local embedding would be better
            .Build();

        // Register functions
        Kernel.ImportFunctions(new TimeContextPlugins(), "Time");
        Kernel.ImportFunctions(new WeatherPlugin(), "Weather");
        Kernel.ImportFunctions(new LatLongPlugin(), "LatLong");
        Kernel.ImportFunctions(new MePlugin(), "User");
        //        Kernel.ImportFunctions(new CameraPlugin(), "Camera"); // Works, but its presence flags content filtering on sexual content

        if (Settings.SupportsAiServices)
        {
            Kernel.ImportFunctions(new VisionPlugin(Settings.AzureAiServicesEndpoint, Settings.AzureAiServicesKey), "Vision");
        }

        if (Settings.SupportsSearch)
        {
            BingConnector searchConnector = new(Settings.BingKey!);
            Kernel.ImportFunctions(new WebSearchEnginePlugin(searchConnector), "Search");
        }

        if (Settings.SupportsSessionize)
        {
            Kernel.ImportFunctions(new SessionizePlugin(null, Settings.SessionizeToken!), "Sessionize");
        }

        // Set up the planner
        PlannerStrategy planStrategy = ChangePlannerCommand.SelectPlanner(Skin);
        Planner = planStrategy.BuildPlanner(Kernel);

        // Show plugins now that they're paying attention
        KernelPluginsRenderer.RenderKernelPluginsChart(Kernel, Skin, this);

        // Primary loop
        AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]System Ready[/]");
        Menus.Push(new RootMenu(this));
        await RunMainLoopAsync();

        // Indicate success on exit (for a fun time, ask me why I always log when a program completes)
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[{Skin.SuccessStyle}]Program complete[/]");

        return 0;
    }

    public Stack<MenuBase> Menus { get; } = new();
    public KernelSettings Settings { get; } = new();
    public SpeechProvider? Speech { get; private set; }
    public PlanExecutionResult? LastResult { get; private set; }
    public Plan? LastPlan { get; private set; }
    public Planner? Planner { get; internal set; }

    private async Task RunMainLoopAsync()
    {
        AnsiConsole.WriteLine();

        while (Menus.TryPeek(out MenuBase? activeMenu) && Kernel != null)
        {
            SelectionPrompt<AppCommand> choices = new SelectionPrompt<AppCommand>()
                    .Title($"[{Skin.NormalStyle}]Select an action[/]")
                    .HighlightStyle(Skin.AccentStyle)
                    .AddChoices(activeMenu.Commands.Where(c => c.CanExecute()))
                    .UseConverter(c => c.DisplayText);

            AppCommand choice = AnsiConsole.Prompt(choices);

            await choice.ExecuteAsync();
        }
    }

    public Task SpeakAsync(string message) => Speech?.SpeakAsync(message) ?? Task.CompletedTask;

    public void Dispose() => Speech?.Dispose();

    public void ReportTokenUsage(int promptTokens, int completionTokens)
    {
        TokenUsageWidget widget = new(new TokenUsage(promptTokens, TokenUsageType.Prompt), 
                                      new TokenUsage(completionTokens, TokenUsageType.Completion));

        widget.Render(Skin);
    }


    public async Task SendUserQueryAsync(string prompt)
    {
        if (Kernel == null)
        {
            throw new InvalidOperationException("Kernel must be initialized");
        }

        try
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[{Skin.NormalStyle}]Executing…[/]");
            AnsiConsole.WriteLine();
            await ExecuteAsync(prompt);
        }
        catch (SKException ex)
        {
            // It's possible to reach the limits of what's possible with the planner. When that happens, handle it gracefully
            if (ex.Message.Contains("Not possible to create plan for goal", StringComparison.OrdinalIgnoreCase) ||
                ex.Message.Contains("Unable to create plan for goal with available functions", StringComparison.OrdinalIgnoreCase))
            {
                Skin.WriteErrorLine("It was not possible to fulfill this request with the available skills.");
            }
            else if (ex.Message.Contains("History is too long", StringComparison.OrdinalIgnoreCase))
            {
                Skin.WriteErrorLine("The planner caused too many tokens to be used to fulfill the request. There may be too many functions enabled.");
            }
            else if (ex.Message.Contains("Missing value for parameter"))
            {
                Skin.WriteErrorLine(ex.Message);
            }
            else
            {
                Skin.WriteException(ex);
            }
        }
        catch (InvalidCastException ex)
        {
            // Invalid Cast can happen with llamaSharp
            Skin.WriteException(ex);
            Skin.WriteErrorLine("Could not generate a plan.");
        }
        catch (Exception ex)
        {
            Skin.WriteException(ex);
        }
    }

    private async Task<string> ExecuteAsync(string userText)
    {
        if (Kernel == null)
        {
            throw new InvalidOperationException("Kernel must be initialized");
        }

        LastPlan = null;
        LastResult = null;

        Plan plan = Planner is null
            ? new Plan(userText)
            : await Planner.CreatePlanAsync(userText);

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
                Output = GetOutputFromResponseContent(ex.ResponseContent) ?? ex.Message
            };
        }

        LastResult = executionResult;
        output = executionResult.Output;

        return output ?? "An unknown error occurred";
    }

    private static string? GetOutputFromResponseContent(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        // TODO: Azure.AI.OpenAI has some objects that this could be deserialized with
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

    public bool IsFunctionExcluded(FunctionView f)
        => f.PluginName.Contains("_Excluded", StringComparison.OrdinalIgnoreCase);
}
