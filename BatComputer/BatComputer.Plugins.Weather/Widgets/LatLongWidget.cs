using MattEland.BatComputer.Abstractions;

namespace BatComputer.Plugins.Weather.Widgets;

public class LatLongWidget : WidgetBase
{
    public LatLongWidget() : this("Lat / Long")
    {

    }

    public LatLongWidget(string title) : base(title)
    {

    }

    public required decimal Latitude { get; set; }
    public required decimal Longitude { get; set; }

    public override void UseSampleData()
    {
        SetTitle("Widgetville Lat / Long");
        Latitude = 42.1124m;
        Longitude = -87.5430m;
    }
}