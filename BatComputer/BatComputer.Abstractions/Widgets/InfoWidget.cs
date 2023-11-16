namespace MattEland.BatComputer.Abstractions.Widgets;

public class InfoWidget : WidgetBase
{
    public InfoWidget(string message)
    {
        this.Title = "Info";
        this.Message = message;
    }

    public InfoWidget(string title, string message)
    {
        this.Title = title;
        this.Message = message;
    }

    public string Message { get; set; }

    public override void UseSampleData()
    {
        this.Title = "Disclaimer";
        this.Message = "Do not read this disclaimer";
    }

    public override string ToString() => $"{Title}: {Message}";
}