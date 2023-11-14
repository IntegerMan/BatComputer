using MattEland.BatComputer.Abstractions;

namespace BatComputer.Plugins.Weather.Widgets;

public class LatLongWidget : WidgetBase
{
    public LatLongWidget(string title) : base(title)
    {

    }

    public required decimal Latitude { get; init; }
    public required decimal Longitude { get; init; }
}