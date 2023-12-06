using MattEland.BatComputer.ConsoleApp.Abstractions;
using MattEland.BatComputer.Kernel;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MattEland.BatComputer.ConsoleApp;

public class BatComputerLogger : ILogger, IDisposable
{
    private readonly BatComputerApp _app;
    private readonly StreamWriter? _writer;
    private readonly Regex _tokensRegEx;
    private readonly Regex _stepwiseRegEx;

    public BatComputerLogger(BatComputerApp app, string filePath = "batcomputer.log")
    {
        _app = app;

        // Set up the regex for finding tokens counts
        _tokensRegEx = new Regex(@"Prompt tokens: (\d+). Completion tokens: (\d+). Total tokens: (\d+).", RegexOptions.IgnoreCase);
        _stepwiseRegEx = new Regex(@"(?:Thought|Action|Observation|Final Answer):\s*(.*)", RegexOptions.IgnoreCase);

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

    public ConsoleSkin Skin => _app.Skin;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        string prefix = GetLogLevelPrefix(logLevel);
        string message = formatter(state, exception);
        string logLine = $"{prefix,5}: {message}";

        _writer?.WriteLine(logLine);


        // Extract token counts from the message
        Match tokenUsageMatch = _tokensRegEx.Match(message);
        if (tokenUsageMatch.Success)
        {
            int promptTokens = int.Parse(tokenUsageMatch.Groups[1].Value);
            int completionTokens = int.Parse(tokenUsageMatch.Groups[2].Value);

            _app.ReportTokenUsage(promptTokens, completionTokens);
        }
        else if (logLevel >= LogLevel.Information)
        {
            if (message.StartsWith("StepwisePlanner_Excluded.ExecutePlan:", StringComparison.OrdinalIgnoreCase) || 
                message.Contains("GenerateEmbeddingsAsync", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            Match stepwiseMatch = _stepwiseRegEx.Match(message);
            if (stepwiseMatch.Success)
            {
                string groupLabel = stepwiseMatch.Groups[0].Value;
                int colonIndex = groupLabel.IndexOf(':');
                if (colonIndex >= 0)
                {
                    groupLabel = groupLabel.Substring(0, colonIndex);
                }

                string stepwiseMessage = stepwiseMatch.Groups[1].Value;
                string markup = $"[{Skin.NormalStyle}]{Markup.Escape(groupLabel)}:[/] [{Skin.DebugStyle}]{Markup.Escape(stepwiseMessage)}[/]";

                if (groupLabel == "Action")
                {
                    AnsiConsole.Write(new Rule(markup).LeftJustified().RuleStyle(Skin.AccentStyle));
                }
                else
                {
                    AnsiConsole.MarkupLine(markup);
                }

                AnsiConsole.WriteLine();
            }
            else
            {
                AnsiConsole.WriteLine(message);
           }

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

#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
    public IDisposable? BeginScope<TState>(TState state)
        => null;
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).

    public void Dispose() => _writer?.Dispose();

    public void Flush() => _writer?.Flush();
}