namespace MattEland.BatComputer.Abstractions.Widgets;

public abstract class WidgetBase : IWidget
{
    public abstract void UseSampleData();

    public string Title { get; set;  } = null!;

    public override string ToString() => Title ?? GetType().Name;
}