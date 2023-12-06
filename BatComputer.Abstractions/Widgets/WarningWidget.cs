namespace MattEland.BatComputer.Abstractions.Widgets;

public class WarningWidget : WidgetBase
{
    public WarningWidget() : this("Warning", "Uh, oh")
    {
    }

    public WarningWidget(string message) : this("Warning", message)
    {
    }

    public WarningWidget(string title, string message)
    {
        this.Title = title;
        this.Message = message;
    }

    public string Message { get; set; }

    public override void UseSampleData()
    {
        this.Title = "Warning";
        this.Message = "Dude, this is just not good.";
    }

    public override string ToString() => $"{Title}: {Message}";
}