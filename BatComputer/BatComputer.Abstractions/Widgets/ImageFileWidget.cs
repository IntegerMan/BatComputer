using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        Title = "TestImage.jpeg";
        Path = "TestImage.jpeg";
    }
}
