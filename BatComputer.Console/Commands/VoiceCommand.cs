using MattEland.BatComputer.ConsoleApp.Helpers;
using MattEland.BatComputer.Kernel;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class VoiceCommand : AppCommand
{
    public override bool CanExecute(AppKernel kernel) => App.Settings.SupportsAiServices && App.Speech != null;

    public override async Task ExecuteAsync(AppKernel kernel)
    {
        string? prompt = null;
        await AnsiConsole.Status().StartAsync("Listening…", async ctx =>
        {
            ctx.Spinner = Skin.Spinner;

            prompt = await App.Speech!.RecognizeAsync();
        });

        if (!string.IsNullOrWhiteSpace(prompt))
        {
            AnsiConsole.MarkupLine($"[{Skin.NormalStyle}]I heard:[/] {Markup.Escape(prompt)}");
            AnsiConsole.WriteLine();

            ConsolePlanExecutor executor = new(App, kernel);
            string? response = await executor.GetKernelPromptResponseAsync(prompt);

            OutputHelpers.DisplayChatResponse(App, response);
        }
        else
        {
            Skin.DisplayWarning("No speech was detected. Check your mic settings and AI services configuration and try again");
        }
    }

    public override string DisplayText => $"Speak a command";

    public VoiceCommand(BatComputerApp app) : base(app)
    {
    }
}
