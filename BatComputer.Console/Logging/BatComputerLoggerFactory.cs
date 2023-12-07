using MattEland.BatComputer.Kernel;
using Microsoft.Extensions.Logging;

namespace MattEland.BatComputer.ConsoleApp.Logging;

public class BatComputerLoggerFactory : ILoggerFactory
{
    private readonly BatComputerApp _app;
    private readonly BatComputerLogger _logger;

    public BatComputerLoggerFactory(BatComputerApp app)
    {
        _app = app;
        _logger = new BatComputerLogger(app);
    }

    public void Dispose() => _logger.Dispose();

    public ILogger CreateLogger(string categoryName) => _logger;

    public void AddProvider(ILoggerProvider provider) => throw new NotSupportedException();

    public void Flush() => _logger.Flush();
}