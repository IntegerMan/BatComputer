using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace MattEland.BatComputer.Kernel.Plugins;

public sealed class TimeContextPlugins
{
    [SKFunction, Description("Gets the current time")]
    public static string GetCurrentTime()
        => $"The current time is {DateTime.Now:T}";

    [SKFunction, Description("Gets the current date including the year, month, day, and day of week")]
    public static string GetCurrentDate()
        => $"The current date is {DateTime.Today:D}";
}