using MattEland.BatComputer.ConsoleApp.Helpers;
using MattEland.BatComputer.Kernel;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class ChatCommand : AppCommand
{
    public override async Task ExecuteAsync(AppKernel kernel)
    {
        string prompt = InputHelpers.GetUserText($"[{Skin.NormalStyle}]Type your request:[/]", addEmptyLine: false);

        if (!string.IsNullOrWhiteSpace(prompt))
        {
            ConsolePlanExecutor executor = new(App, kernel);
            string? response = await executor.GetKernelPromptResponseAsync(prompt);

            OutputHelpers.DisplayChatResponse(App, response);
        }
    }

    public override string DisplayText => $"Query {Skin.AppNameWithPrefix}";

    public ChatCommand(BatComputerApp app) : base(app)
    {
    }
}
