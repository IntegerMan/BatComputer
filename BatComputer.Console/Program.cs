using Spectre.Console;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MattEland.BatComputer.ConsoleApp;

internal class Program {
    private static async Task<int> Main() {

        try {
            // Using UTF8 allows more capabilities for Spectre.Console.
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            // Good for full screening before the main window shows. Use that for screen grabs and demos: Console.ReadKey(true);

            using BatComputerApp app = new();
            return await app.RunAsync();
        }
        catch (Exception ex) {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            return -1;
        }
    }


}