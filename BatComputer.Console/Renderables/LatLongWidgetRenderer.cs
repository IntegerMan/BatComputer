﻿using MattEland.BatComputer.ConsoleApp.Abstractions;
using MattEland.BatComputer.Plugins.Weather.Widgets;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Renderables;

// ReSharper disable once UnusedMember.Global - Used via Reflection
public class LatLongWidgetRenderer : WidgetRenderer<LatLongWidget>
{
    public override void Render(LatLongWidget widget, ConsoleSkin skin)
    {
        Table table = new Table().AddColumns($"[{skin.NormalStyle}]Lat[/]", $"[{skin.NormalStyle}]Long[/]")
            .Title(widget.ToString(), style: skin.AccentStyle)
            .Centered()
            .AddRow(widget.Latitude.ToString("0.0000"), widget.Longitude.ToString("0.0000"));

        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }
}
