using BatComputer.Plugins.Weather.Widgets;
using MattEland.BatComputer.ConsoleApp.Abstractions;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Renderables;

// ReSharper disable once UnusedMember.Global - Used via Reflection
public class CurrentWeatherWidgetRenderer : WidgetRenderer<CurrentWeatherWidget>
{
    public override void Render(CurrentWeatherWidget widget, ConsoleSkin skin)
    {
        Table table = new Table()
            .AddColumns("","")
            .HideHeaders()
            .AddRow($"[{skin.NormalStyle}]Conditions:[/]", $"{widget.Conditions ?? "None"}, {(widget.IsDay ? "Day" : "Night")}")
            .AddRow($"[{skin.NormalStyle}]Temperature:[/]", widget.Temperature)
            .AddRow($"[{skin.NormalStyle}]Clouds:[/]", widget.CloudCover)
            .AddRow($"[{skin.NormalStyle}]Rainfall:[/]", widget.Rainfall)
            .AddRow($"[{skin.NormalStyle}]Snowfall:[/]", widget.Snowfall)
            .Title(widget.ToString(), style: skin.AccentStyle)
            .Centered();
        AnsiConsole.Write(table);
        AnsiConsole.WriteLine();
    }
}
