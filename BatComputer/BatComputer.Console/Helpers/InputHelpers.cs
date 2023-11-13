using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Helpers;

public static class InputHelpers
{
    public static string GetUserText(string message)
    {
        string prompt = AnsiConsole.Ask<string>(message);

        AnsiConsole.WriteLine();

        return prompt;
    }
}
