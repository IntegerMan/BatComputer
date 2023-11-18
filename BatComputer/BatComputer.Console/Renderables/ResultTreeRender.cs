using MattEland.BatComputer.Abstractions.Strategies;
using MattEland.BatComputer.ConsoleApp.Abstractions;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp.Renderables;

public static class ResultTreeRenderer
{
    public static void RenderTree(this PlanExecutionResult result, ConsoleSkin skin)
    {
        Tree tree = BuildTree(result, skin);

        AnsiConsole.Write(tree);
        AnsiConsole.WriteLine();
    }

    private static Tree BuildTree(PlanExecutionResult result, ConsoleSkin skin)
    {
        Tree planTree = new($"[{skin.NormalStyle}]Execution Result[/]");

        PopulateTree(result, skin, planTree);

        return planTree;
    }

    private static void PopulateTree(PlanExecutionResult result, ConsoleSkin skin, IHasTreeNodes tree)
    {
        tree.AddNode($"[{skin.NormalStyle}]Iterations[/]: [{skin.DebugStyle}]{result.Iterations} [/]");
        //tree.AddNode($"[{skin.NormalStyle}]Steps[/]: [{skin.AccentStyle}] {result.StepsCount} [/]");
        //tree.AddNode($"[{skin.NormalStyle}]Functions[/]: [{skin.AccentStyle}] {result.FunctionsUsed} [/]");

        int index = 1;
        foreach (StepwiseSummary summary in result.Summary)
        {
            if (string.IsNullOrEmpty(summary.Thought)) continue;

            TreeNode node = tree.AddNode($"[{skin.NormalStyle}]Step {index++}[/]: [{skin.AccentStyle}]{Markup.Escape(summary.Action ?? "None")}[/]");
            node.AddNode($"[{skin.AccentStyle}]Thought[/]: [{skin.DebugStyle}] {Markup.Escape(summary.Thought ?? "None")} [/]");

            if (summary.ActionVariables.Any())
            {
                TreeNode varNode = node.AddNode($"[{skin.NormalStyle}]Variables[/]");
                foreach (KeyValuePair<string, string> kvp in summary.ActionVariables)
                {
                    varNode.AddNode($"[{skin.AccentStyle}]{Markup.Escape(kvp.Key)}[/]: [{skin.DebugStyle}]{Markup.Escape(kvp.Value.GetValueText())}[/]");
                }
            }

            node.AddNode($"[{skin.AccentStyle}]Observation[/]: [{skin.DebugStyle}] {GetValueText(summary.Observation ?? "None")} [/]");            
        }

        tree.AddNode($"[{skin.NormalStyle}]Output[/]: [{skin.SuccessStyle}] {GetValueText(result.Output ?? "None")} [/]");
    }

    private static string GetValueText(this string value)
    {
        const int MaxShortLength = 140;

        if (value.StartsWith("<!DOCTYPE html>", StringComparison.OrdinalIgnoreCase) && value.Length > MaxShortLength)
        {
            return "<!HTML>";
        }
        else if (value.StartsWith('{') && value.EndsWith('}') && value.Length > MaxShortLength)
        {
            return "{json}";
        }
        else if (value.StartsWith('[') && value.EndsWith(']') && value.Length > MaxShortLength)
        {
            return Markup.Escape("[json]");
        }
        else if (value.StartsWith('<') && value.EndsWith('>') && value.Length > MaxShortLength)
        {
            return "<XML />";
        }
        else
        {
            return Markup.Escape(value);
        }
    }
}
