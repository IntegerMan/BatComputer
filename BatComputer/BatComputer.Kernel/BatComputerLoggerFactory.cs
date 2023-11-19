using Microsoft.Extensions.Logging;

namespace MattEland.BatComputer.Kernel;

public class BatComputerLoggerFactory : ILoggerFactory
{
    private readonly AppKernel _app;
    private readonly BatComputerLogger _logger;

    public BatComputerLoggerFactory(AppKernel app)
    {
        _app = app;
        _logger = new BatComputerLogger(app);
    }

    public void Dispose() => _logger.Dispose();

    public ILogger CreateLogger(string categoryName) => _logger;

    public void AddProvider(ILoggerProvider provider) => throw new NotSupportedException();

    public void Flush() => _logger.Flush();
}