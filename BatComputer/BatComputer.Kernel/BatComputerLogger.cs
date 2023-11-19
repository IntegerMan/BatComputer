using Microsoft.Extensions.Logging;

namespace MattEland.BatComputer.Kernel;

public class BatComputerLogger : ILogger
{
    private readonly AppKernel _app;

    public BatComputerLogger(AppKernel app)
    {
        _app = app;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        Console.WriteLine($"*** {logLevel} *** {formatter(state, exception)}");
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => null;
}