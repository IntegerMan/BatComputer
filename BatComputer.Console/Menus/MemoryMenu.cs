using MattEland.BatComputer.ConsoleApp.Commands;
using Microsoft.SemanticKernel.Memory;

namespace MattEland.BatComputer.ConsoleApp.Menus;

public class MemoryMenu : MenuBase
{
    public MemoryMenu(BatComputerApp app, ISemanticTextMemory memory, string collectionName) : base(app)
    {
        Memory = memory;
        CollectionName = collectionName;
    }

    public override IEnumerable<AppCommand> Commands
    {
        get
        {
            yield return new ShowMemoryCommand(App, Memory);
            yield return new SearchMemoryCommand(App, CollectionName);
            yield return new AddMemoryCommand(App, CollectionName, Memory);

            yield return new ExitCommand(App, title: "Back");
        }
    }

    public ISemanticTextMemory Memory { get; }
    public string CollectionName { get; }
}
