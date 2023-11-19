using System.ComponentModel;
using MattEland.BatComputer.Abstractions.Widgets;

namespace MattEland.BatComputer.Plugins.Weather.Widgets;

public class CurrentWeatherWidget : WidgetBase
{
    public string? Conditions { get; set; } = "Not detected";
    public string Temperature { get; set; } = "0\u00b0F";
    public string ApparentTemperature { get; set; } = "0\u00b0F";
    [Description("Cloud Cover")] public string CloudCover { get; set; } = "0%";

    public bool IsDay { get; set; }
    public string Rainfall { get; set; } = "0.0 inches";
    public string Snowfall { get; set; } = "0.0 inches";

    public override void UseSampleData()
    {
        Title = "Widgetville Weather";
        IsDay = true;
        Snowfall = "3.5 inches";
        Rainfall = "0.0 inches";
        CloudCover = "42%";
        Conditions = "Partly Cloudy";
        Temperature = "28\u00b0F";
        ApparentTemperature = "26\u00b0F";
    }
}