using System.ComponentModel;
using MattEland.BatComputer.Abstractions;

namespace BatComputer.Plugins.Weather.Widgets;

public class CurrentWeatherWidget : WidgetBase
{
    public CurrentWeatherWidget(string title = "Current Weather") : base(title)
    {

    }

    public string? Conditions { get; set; } = "Not detected";
    public required string Temperature { get; init; }
    [Description("Cloud Cover")]
    public required string CloudCover { get; init; }

    public bool IsDay { get; init; }
    public required string Rainfall { get; init; }
    public required string Snowfall { get; init; }
}