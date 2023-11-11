using Spectre.Console;

internal class Program {
    private static void Main(string[] args) {
        AnsiConsole.WriteLine("Welcome to the");
        AnsiConsole.Write(new FigletText("Bat Computer")
            .Centered()
            .Color(Color.Yellow));
        AnsiConsole.Write(new Text("Version 0.0.1"));
    }
}