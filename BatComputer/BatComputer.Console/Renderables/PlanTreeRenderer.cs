using MattEland.BatComputer.Kernel;
using Microsoft.SemanticKernel.Planning;
using Spectre.Console;

namespace MattEland.BatComputer.ConsoleApp;

public static class PlanTreeRenderer
{
    public static void RenderTree(this Plan plan, ConsoleSkin skin)
    {
        Tree tree = BuildTree(plan, skin);

        AnsiConsole.Write(tree);
        AnsiConsole.WriteLine();
    }

    private static Tree BuildTree(Plan plan, ConsoleSkin skin)
    {
        Tree planTree = new($"[{skin.NormalStyle}]{plan.PluginName}[/]:[{skin.AccentStyle}]{plan.Name}[/]");

        string target = plan.GetTarget();

        PopulateTree(plan, skin, planTree, target);

        return planTree;
    }

    private static void PopulateTree(Plan plan, ConsoleSkin skin, IHasTreeNodes tree, string target)
    {
        if (plan.Parameters.Any())
        {
            TreeNode parameters = tree.AddNode("Parameters");
            foreach (KeyValuePair<string, string> param in plan.Parameters)
            {
                parameters.AddNode($":incoming_envelope: [{skin.NormalStyle}]{param.Key}[/]");
            }
        }

        if (plan.Steps.Any())
        {
            TreeNode steps = tree.AddNode("Steps");
            foreach (Plan step in plan.Steps)
            {
                TreeNode stepNode = steps.AddNode($"[{skin.NormalStyle}]{step.Name}[/]");

                PopulateTree(step, skin, stepNode, target);
            }
        }

        if (plan.State.Any())
        {
            TreeNode state = tree.AddNode("State");

            foreach (KeyValuePair<string, string> kvp in plan.State)
            {
                // Plan.Result tends to be every other answer joined together, so displaying it is usually meaningless / noisy
                if (kvp.Key == "PLAN.RESULT")
                    continue;

                if (kvp.Key == target)
                {
                    state.AddNode($"[{skin.AccentStyle}]{kvp.Key}[/]:[{skin.NormalStyle}]{kvp.Value}[/]");
                }
                else
                {
                    state.AddNode($"[{skin.NormalStyle}]{kvp.Key}[/]:[{skin.DebugStyle}]{kvp.Value}[/]");
                }
            }
        }

        if (plan.Outputs.Any())
        {
            TreeNode outputs = tree.AddNode("Outputs");
            foreach (string output in plan.Outputs)
            {
                if (output == target)
                {
                    outputs.AddNode($":goal_net: [{skin.AccentStyle}]{output}[/]");
                }
                else
                {
                    outputs.AddNode($":outbox_tray: [{skin.NormalStyle}]{output}[/]");
                }
            }
        }
    }

}
