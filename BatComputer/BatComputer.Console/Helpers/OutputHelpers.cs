using Dumpify;
using MattEland.BatComputer.Abstractions;
using MattEland.BatComputer.ConsoleApp.Abstractions;
using MattEland.BatComputer.ConsoleApp.Renderables;
using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Json;
using System.Reflection;
using MattEland.BatComputer.Abstractions.Widgets;

namespace MattEland.BatComputer.ConsoleApp.Helpers;

public static class OutputHelpers
{
    public static void RenderJson(this object obj)
    {
        AnsiConsole.Write(new JsonText(JsonConvert.SerializeObject(obj)));
        AnsiConsole.WriteLine();
    }

    public static void DisplayChatResponse(BatComputerApp app, IAppKernel kernel, string chatResponse)
    {
        while (kernel.Widgets.Any())
        {
            IWidget widget = kernel.Widgets.Dequeue();
            widget.Render(app.Skin);
        }

        AnsiConsole.MarkupLine($"[{app.Skin.AgentStyle}]{Markup.Escape(app.Skin.AgentName)}: {Markup.Escape(chatResponse)}[/]");
        AnsiConsole.WriteLine();
    }

    public static void Render(this IWidget widget, ConsoleSkin skin)
    {
        Type widgetType = widget.GetType();
        Type widgetRendererType = typeof(WidgetRenderer<>).MakeGenericType(widgetType);

        Type[] types = Assembly.GetExecutingAssembly().GetTypes();

        foreach (Type type in types)
        {
            if (type.IsSubclassOf(widgetRendererType))
            {
                Type genericType = type.BaseType!.GetGenericArguments()[0];
                if (genericType == widgetType)
                {
                    var renderer = Activator.CreateInstance(type);
                    MethodInfo renderMethod = type.GetMethod("Render")!;
                    renderMethod.Invoke(renderer, new object[] { widget, skin });
                    return;
                }
            }
        }

        // No renderer found for it
        RenderDump(widget);
    }

    public static void RenderImage(string path, int? maxWidth = 20)
    {
        CanvasImage image = new(path);
        image.MaxWidth = maxWidth;

        AnsiConsole.Write(image);
    }

    public static void RenderDump(IWidget widget)
    {
        widget.Dump(label: widget.ToString(),
            typeNames: new TypeNamingConfig { ShowTypeNames = false },
            tableConfig: new TableConfig { ShowTableHeaders = false },
            members: new MembersConfig { IncludeFields = false, IncludeNonPublicMembers = false });
    }
}
