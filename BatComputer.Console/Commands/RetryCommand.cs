using MattEland.BatComputer.ConsoleApp.Helpers;
using MattEland.BatComputer.Kernel;

namespace MattEland.BatComputer.ConsoleApp.Commands;

public class RetryCommand : AppCommand
{
    public override async Task ExecuteAsync(AppKernel kernel)
    {
        string? response;

        ConsolePlanExecutor executor = new(App, kernel);
        response = await executor.GetKernelPromptResponseAsync(kernel.LastMessage!);

        OutputHelpers.DisplayChatResponse(App, kernel, response);
    }

    public override string DisplayText => "Repeat last request";

    public override bool CanExecute(AppKernel kernel) => kernel.LastMessage != null;

    public RetryCommand(BatComputerApp app) : base(app)
    {
    }
}
