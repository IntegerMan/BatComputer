using Dumpify;
using MattEland.BatComputer.Abstractions;
using Newtonsoft.Json;
using Spectre.Console;
using Spectre.Console.Json;

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
            widget.Render();
        }

        AnsiConsole.MarkupLine($"[{app.Skin.AgentStyle}]{Markup.Escape(app.Skin.AgentName)}: {Markup.Escape(chatResponse)}[/]");
        AnsiConsole.WriteLine();
    }

    public static void Render(this IWidget widget)
    {
        widget.Dump(label: widget.ToString(),
            typeNames: new TypeNamingConfig {ShowTypeNames = false},
            tableConfig: new TableConfig {ShowTableHeaders = false},
            members: new MembersConfig {IncludeFields = false, IncludeNonPublicMembers = false});
    }
}
