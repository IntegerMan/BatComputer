using MattEland.BatComputer.ConsoleApp.Helpers;
using MattEland.BatComputer.Kernel;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class SemanticQueryCommand : AppCommand
{
    public override async Task ExecuteAsync(AppKernel kernel)
    {
        string prompt = InputHelpers.GetUserText($"[{Skin.NormalStyle}]Type your request:[/]");

        if (!string.IsNullOrWhiteSpace(prompt))
        {
            ConsolePlanExecutor executor = new(App, kernel);
            string? response = await executor.GetKernelPromptResponseAsync(prompt);

            OutputHelpers.DisplayChatResponse(App, kernel, response);
        }
    }

    public override string DisplayText => $"Query {Skin.AppNameWithPrefix}";

    public SemanticQueryCommand(BatComputerApp app) : base(app)
    {
    }
}
