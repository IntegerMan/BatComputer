using MattEland.BatComputer.Abstractions;

namespace BatComputer.Plugins.Weather.Widgets;

public class LatLongWidget : WidgetBase
{
    public required decimal Latitude { get; set; }
    public required decimal Longitude { get; set; }

    public override void UseSampleData()
    {
        Title = "Widgetville Lat / Long";
        Latitude = 42.1124m;
        Longitude = -87.5430m;
    }
}