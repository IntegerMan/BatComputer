using MattEland.BatComputer.Kernel;
using Microsoft.SemanticKernel;
using Spectre.Console;
using System.Text;

namespace MattEland.BatComputer.ConsoleApp.Renderables;
public static class KernelPluginsRenderer
{
    public static void RenderKernelPluginsChart(this BatKernel kernel, ConsoleSkin skin)
    {
        List<FunctionView> funcs = GetActiveFunctions(kernel);

        string headerMarker = $"[{skin.SuccessStyle}]{funcs.Count} Plugin Functions Detected[/]";

        IOrderedEnumerable<IGrouping<string, FunctionView>> funcsByPlugin =
            funcs.GroupBy(f => f.PluginName)
                    .OrderByDescending(g => g.Count())
                    .ThenBy(g => g.Key);

        AnsiConsole.Write(new BarChart()
            .Label(headerMarker)
            .LeftAlignLabel()
            .AddItems(funcsByPlugin, item => new BarChartItem(item.Key, item.Count()))); // TODO: Colorize by value

        AnsiConsole.WriteLine();
    }

    public static void RenderKernelPluginsTable(this BatKernel kernel, ConsoleSkin skin)
    {
        List<FunctionView> funcs = GetActiveFunctions(kernel);

        string headerMarker = $"[{skin.SuccessStyle}]{funcs.Count} Plugin Functions Detected[/]";

        AnsiConsole.MarkupLine(headerMarker);

        Table funcTable = new();
        funcTable.AddColumns("Name", "Parameters", "Description");
        foreach (FunctionView funcView in funcs)
        {
            string parameters = string.Join(", ", funcView.Parameters.Select(p =>
            {
                StringBuilder sb = new(p.Name);

                if (p.IsRequired != true)
                {
                    sb.Append('?');
                }

                if (!string.IsNullOrEmpty(p.DefaultValue))
                {
                    sb.Append(" = " + p.DefaultValue);
                }

                return sb.ToString();
            }));
            string qualifiedName = $"[{skin.NormalStyle}]{funcView.PluginName}[/]:[{skin.AccentStyle}]{funcView.Name}[/]";

            funcTable.AddRow(qualifiedName, parameters, funcView.Description);
        }

        AnsiConsole.Write(funcTable);
        AnsiConsole.WriteLine();
    }

    private static List<FunctionView> GetActiveFunctions(BatKernel kernel)
    {
        IReadOnlyList<FunctionView> functions = kernel.Kernel.Functions.GetFunctionViews();

        List<FunctionView> funcs = functions.Where(f => !kernel.IsFunctionExcluded(f))
                                            .OrderBy(f => f.PluginName)
                                            .ThenBy(f => f.Name)
                                            .ToList();
        return funcs;
    }
}
