using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Helpers;

public static class InputHelpers
{
    public static string GetUserText(string message)
    {
        string value = AnsiConsole.Ask<string>(message);
        AnsiConsole.WriteLine();

        return value;
    }
}
