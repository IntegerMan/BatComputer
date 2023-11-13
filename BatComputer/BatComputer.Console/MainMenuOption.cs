using System.ComponentModel;

namespace MattEland.BatComputer.ConsoleApp;

public enum MainMenuOption
{
    Query,
    Chat,
    [Description("List Active Plugins")]
    ListPlugins,
    [Description("Show last plan tree")]
    ShowPlanTree,
    [Description("Show last plan JSON")]
    ShowPlanJson,
    Quit
}