using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Orchestration;
using System.ComponentModel;

namespace MattEland.BatComputer.Kernel;

public class ChatPlugin
{
    private const string SystemText = "You are an AI assistant named Alfred, the virtual butler to Batman. The user is Batman.";

    private readonly IKernel _kernel;
    private ISKFunction _chatFunc;

    public ChatPlugin(IKernel kernel)
    {
        _kernel = kernel;
        string prompt = @"Bot: How can I help you?
User: {{$input}}

---------------------------------------------

Bot: ";
        _chatFunc = kernel.CreateSemanticFunction(prompt, new OpenAIRequestSettings() { ChatSystemPrompt = SystemText, ResultsPerPrompt = 1 }, "Chat", "SemanticFunctions", "Gets a response to a conversational query");
    }

    [SKFunction, Description("Gets a response to a question")]
    public async Task<string> GetChatResponse([Description("The text the user typed in with their query")] string prompt)
    {
        KernelResult result = await _kernel.RunAsync(prompt, _chatFunc);
        return result.GetValue<string>() ?? "";
    }
}
