using BatComputer.Abstractions;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp;
public class SpectreLogger : ILogger
{
    public SpectreLogger(ConsoleSkin skin)
    {
        Skin = skin;
    }

    public ConsoleSkin Skin { get; }

    public IDisposable BeginScope<TState>(TState state)
    {
        throw new NotSupportedException();
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string text = formatter.Invoke(state, exception);

        if (ShouldLogMessage(text))
        {
            AnsiConsole.MarkupLine($"[{Skin.DebugStyle}]{logLevel}: {Markup.Escape(text)}[/]");
        }
    }

    private static bool ShouldLogMessage(string text)
    {
        return !text.Contains("Rendering string template") &&
                    !text.Contains("Extracting blocks from template") &&
                    !text.Contains("Rendered prompt");
    }
}