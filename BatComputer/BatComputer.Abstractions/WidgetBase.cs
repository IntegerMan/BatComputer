namespace MattEland.BatComputer.Abstractions;

public abstract class WidgetBase : IWidget
{
    public abstract void UseSampleData();

    public required string Title { get; set;  }

    public override string ToString() => Title ?? GetType().Name;
}