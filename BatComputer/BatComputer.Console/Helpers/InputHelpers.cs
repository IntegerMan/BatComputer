using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Helpers;

public static class InputHelpers
{
    public static string GetUserText(string message) 
        => AnsiConsole.Ask<string>(message);
}
