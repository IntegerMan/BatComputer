using MattEland.BatComputer.Abstractions.Widgets;

namespace MattEland.BatComputer.Plugins.Weather.Widgets;

public class LatLongWidget : WidgetBase
{
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }

    public override void UseSampleData()
    {
        Title = "Widgetville Lat / Long";
        Latitude = 42.1124m;
        Longitude = -87.5430m;
    }
}