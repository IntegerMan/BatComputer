using MattEland.BatComputer.ConsoleApp.Helpers;
using MattEland.BatComputer.Kernel;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class ChatCommand : AppCommand
{
    public override async Task ExecuteAsync(AppKernel kernel)
    {
        string prompt = InputHelpers.GetUserText($"[{Skin.NormalStyle}]Type your message:[/]");

        if (!string.IsNullOrWhiteSpace(prompt))
        {
            string? response = null;
            await AnsiConsole.Status().StartAsync("Sending...", async ctx =>
            {
                ctx.Spinner = Skin.Spinner;

                response = await kernel.GetChatPromptResponseAsync(prompt);
            });

            OutputHelpers.DisplayChatResponse(App, kernel, response!);
        }
    }

    public override string DisplayText => $"Chat with {Skin.AgentName}";

    public ChatCommand(BatComputerApp app) : base(app)
    {
    }
}
