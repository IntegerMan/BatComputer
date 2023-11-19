using MattEland.BatComputer.ConsoleApp.Commands;
using MattEland.BatComputer.ConsoleApp.Helpers;
using MattEland.BatComputer.Kernel;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Menus;
public class ExecuteFunctionCommand : AppCommand
{
    private readonly FunctionView _funcView;
    private readonly ISKFunction _skFunction;

    public ExecuteFunctionCommand(BatComputerApp app, FunctionView func, ISKFunction skFunc) : base(app)
    {
        _funcView = func;
        _skFunction = skFunc;
    }

    public override async Task ExecuteAsync(AppKernel kernel)
    {
        try
        {
            SKContext context = App.Kernel!.Kernel.CreateNewContext();

            foreach (ParameterView parameter in _funcView.Parameters)
            {
                string? value = InputHelpers.GetUserText($"[{Skin.NormalStyle}]Enter a value for {parameter.Name}:[/]");
                context.Variables.Set(parameter.Name, value);
            }

            FunctionResult result = await _skFunction.InvokeAsync(context);

            string? output = result.GetValue<string>();
            AnsiConsole.MarkupLine($"[{Skin.DebugStyle}]{Markup.Escape(output ?? "[No result returned]")}[/]");
            AnsiConsole.WriteLine();
        }
        catch (Exception ex)
        {
            OutputHelpers.WriteException(Skin, ex);
        }
    }

    public override string DisplayText => $"Invoke {_funcView.Name}";
}