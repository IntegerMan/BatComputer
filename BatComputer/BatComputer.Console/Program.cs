using Spectre.Console;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace MattEland.BatComputer.ConsoleApp;

internal class Program {
    private static async Task<int> Main() {

        try {
            // Using UTF8 allows more capabilities for Spectre.Console.
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            BatComputerApp app = new();
            return await app.RunAsync();
        }
        catch (Exception ex) {
            AnsiConsole.WriteException(ex, ExceptionFormats.ShortenEverything);
            return -1;
        }
    }


}