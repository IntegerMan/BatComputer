﻿using MattEland.BatComputer.ConsoleApp.Skins;
using Microsoft.Extensions.Logging;

namespace MattEland.BatComputer.ConsoleApp;

public class SpectreLoggerFactory : ILoggerFactory
{
    public SpectreLoggerFactory(ConsoleSkin skin)
    {
        Skin = skin;
    }

    public ConsoleSkin Skin { get; }

    public void AddProvider(ILoggerProvider provider)
    {
        throw new NotSupportedException();
    }

    public ILogger CreateLogger(string categoryName) => new SpectreLogger(Skin);

    public void Dispose()
    {
    }
}