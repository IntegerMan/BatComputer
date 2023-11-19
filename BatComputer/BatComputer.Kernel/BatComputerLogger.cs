using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MattEland.BatComputer.Kernel;

public class BatComputerLogger : ILogger, IDisposable
{
    private readonly AppKernel _app;
    private readonly StreamWriter? _writer;
    private Regex _tokensRegEx;

    public BatComputerLogger(AppKernel app, string filePath = "batcomputer.log")
    {
        _app = app;

        // Set up the regex for finding tokens counts
        string pattern = @"Prompt tokens: (\d+). Completion tokens: (\d+). Total tokens: (\d+).";
        _tokensRegEx = new Regex(pattern, RegexOptions.IgnoreCase);

        try
        {
            _writer = new StreamWriter(File.Open(filePath, FileMode.Create, FileAccess.Write));
        }
        catch (Exception ex)
        {
            // It's fine if we can't open the file for logging. We'll log it to debug, but the app will be fine without the log output
            Debug.WriteLine($"Could not open log file {filePath}: {ex.GetType().Name}, {ex.Message}");
        }
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string prefix = GetLogLevelPrefix(logLevel);
        string message = formatter(state, exception);
        string logLine = $"{prefix,5}: {message}";

        _writer?.WriteLine(logLine);
        Debug.WriteLine(logLine);

        // Extract token counts from the message
        Match match = _tokensRegEx.Match(message);
        if (match.Success)
        {
            int promptTokens = int.Parse(match.Groups[1].Value);
            int completionTokens = int.Parse(match.Groups[2].Value);

            _app.ReportTokenUsage(promptTokens, completionTokens);
            //_app.AddWidget(new TokenUsageWidget(promptTokens, completionTokens, $"Event {eventId.Name} {eventId.Id}"));
        }
    }

    private static string GetLogLevelPrefix(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "TRACE",
            LogLevel.Debug => "DEBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERROR",
            LogLevel.Critical => "CRIT",
            LogLevel.None => "NONE",
            _ => "UNKN",
        };
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => null;

    public void Dispose() => _writer?.Dispose();

    public void Flush() => _writer?.Flush();
}