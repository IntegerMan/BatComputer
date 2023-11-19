using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MattEland.BatComputer.Kernel;

public class BatComputerLoggerFactory : ILoggerFactory
{
    private readonly AppKernel _app;

    public BatComputerLoggerFactory(AppKernel app)
    {
        _app = app;
    }

    public void Dispose()
    {
    }

    public ILogger CreateLogger(string categoryName) => new BatComputerLogger(_app);

    public void AddProvider(ILoggerProvider provider) => throw new NotSupportedException();
}