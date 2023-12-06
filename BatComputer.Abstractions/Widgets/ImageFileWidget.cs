namespace MattEland.BatComputer.Abstractions.Widgets;

public class ImageFileWidget : WidgetBase
{
    public ImageFileWidget() : this(System.IO.Path.GetTempFileName())
    {
    }

    public ImageFileWidget(string fileName)
    {
        Title = fileName;
        Path = fileName;
    }

    public string Path { get; set; }

    public override void UseSampleData()
    {
        Path = System.IO.Path.Combine(Environment.CurrentDirectory, "TestImage.jpeg");
        Title = "TestImage.jpeg";
    }
}
