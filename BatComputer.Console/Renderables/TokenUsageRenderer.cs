using MattEland.BatComputer.Abstractions.Widgets;
using MattEland.BatComputer.ConsoleApp.Abstractions;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Renderables;

// ReSharper disable once UnusedMember.Global - Used via Reflection
public class TokenUsageRenderer : WidgetRenderer<TokenUsageWidget>
{
    public override void Render(TokenUsageWidget widget, ConsoleSkin skin)
    {
        AnsiConsole.Write(new Panel(
                new BreakdownChart()
                        .ShowTagValues()
                        .AddItem("Prompt", widget.PromptTokens, skin.NormalColor)
                        .AddItem("Completion", widget.CompletionTokens, skin.AgentColor))
            .Header($"[{skin.NormalStyle}] {Markup.Escape(widget.Title)} [/]")
            .Padding(2, 1, 2, 0)
            .Border(BoxBorder.Rounded)
            .BorderStyle(skin.AccentStyle));

        AnsiConsole.WriteLine();
    }
}
