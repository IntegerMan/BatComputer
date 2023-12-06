namespace MattEland.BatComputer.Abstractions.Widgets;

public class DataWidget : WidgetBase
{
    public DataWidget() : this ("Object Data", null)
    {

    }

    public DataWidget(string title) : this(title, null)
    {

    }

    public DataWidget(string title, object? data)
    {
        this.Title = title;
        this.Data = data;
    }

    public override void UseSampleData()
    {
        this.Data = new {Name = "Bruce Wayne", Occupation = "Crime Fighter"};
        this.Title = "Object Data";
    }

    public object? Data { get; set; }
}