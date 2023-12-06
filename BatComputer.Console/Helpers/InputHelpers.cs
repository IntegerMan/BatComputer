using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Helpers;

public static class InputHelpers
{
    public static string GetUserText(string message, bool addEmptyLine = true)
    {
        string value = AnsiConsole.Ask<string>(message);
        if (addEmptyLine)
        {
            AnsiConsole.WriteLine();
        }

        return value;
    }
}
