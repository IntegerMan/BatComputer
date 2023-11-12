using MattEland.BatComputer.ConsoleApp.Skins;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MattEland.BatComputer.ConsoleApp.Renderables;
public static class WelcomeRenderer
{
    public static void ShowWelcome(ConsoleSkin skin)
    {
        AnsiConsole.MarkupLine($"[{skin.NormalStyle}]Welcome to {skin.AppNamePrefix}[/]".TrimEnd());

        AnsiConsole.Write(new FigletText(FigletFont.Default, skin.AppName)
            .Centered()
            .Color(skin.NormalColor));

        Version version = Assembly.GetEntryAssembly()!.GetName().Version!;
        AnsiConsole.Write(new Text("By Matt Eland", style: skin.AccentStyle).Centered());
        AnsiConsole.Write(new Text($"Version {version}", style: skin.NormalStyle).RightJustified());
        AnsiConsole.WriteLine();

        AnsiConsole.WriteLine();
    }

}
