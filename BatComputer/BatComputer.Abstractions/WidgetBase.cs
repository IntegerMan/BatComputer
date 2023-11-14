namespace MattEland.BatComputer.Abstractions;

public abstract class WidgetBase : IWidget
{
    private string? _title;

    protected WidgetBase(string? title = null)
    {
        _title = title ?? GetType().Name;
    }

    public void SetTitle(string? title) => _title = title;

    public string? GetTitle() => _title;

    public override string ToString() => _title ?? GetType().Name;
}