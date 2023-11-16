using System.ComponentModel;
using MattEland.BatComputer.Abstractions;

namespace BatComputer.Plugins.Weather.Widgets;

public class CurrentWeatherWidget : WidgetBase
{
    public string? Conditions { get; set; } = "Not detected";
    public required string Temperature { get; set; }
    [Description("Cloud Cover")]
    public required string CloudCover { get; set; }

    public bool IsDay { get; set; }
    public required string Rainfall { get; set; }
    public required string Snowfall { get; set; }

    public override void UseSampleData()
    {
        Title = "Widgetville Weather";
        IsDay = true;
        Snowfall = "3.5 inches";
        Rainfall = "0.0 inches";
        CloudCover = "42%";
        Conditions = "Partly Cloudy";
        Temperature = "28\u00b0F";
    }
}