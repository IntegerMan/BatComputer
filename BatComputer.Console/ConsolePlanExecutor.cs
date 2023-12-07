using MattEland.BatComputer.ConsoleApp.Abstractions;
using MattEland.BatComputer.Kernel;
using Microsoft.SemanticKernel.Diagnostics;
using Spectre.Console;
using MattEland.BatComputer.ConsoleApp.Helpers;

namespace MattEland.BatComputer.ConsoleApp;

public class ConsolePlanExecutor
{
    private readonly BatComputerApp _app;
    private readonly AppKernel _kernel;

    public ConsolePlanExecutor(BatComputerApp app, AppKernel kernel)
    {
        _app = app;
        _kernel = kernel;
    }

    protected ConsoleSkin Skin => _app.Skin;


}
