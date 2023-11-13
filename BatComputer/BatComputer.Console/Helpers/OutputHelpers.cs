using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Json;

namespace MattEland.BatComputer.ConsoleApp.Helpers;

public static class OutputHelpers
{
    public static void RenderJson(this object obj)
    {
        AnsiConsole.Write(new JsonText(JsonConvert.SerializeObject(obj)));
        AnsiConsole.WriteLine();
    }
}
