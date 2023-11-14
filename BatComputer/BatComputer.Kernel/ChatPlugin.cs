using LLamaSharp.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.AI.OpenAI;
using Microsoft.SemanticKernel.Orchestration;
using System.ComponentModel;
using System.Security.Cryptography;
using LLama.Common;
using LLamaSharp.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI.TextCompletion;
using LLamaSharp.SemanticKernel.TextCompletion;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.ChatCompletion;

namespace MattEland.BatComputer.Kernel;

public class ChatPlugin
{
    public const string SystemText = "You are an AI assistant named Alfred, the virtual butler to Batman. The user is Batman.";

    private readonly AppKernel _kernel;

    public ChatPlugin(AppKernel kernel)
    {
        _kernel = kernel;
    }

    [SKFunction, Description("Sends a question to a large language model as part of a chat traanscript")]
    public async Task<string> GetChatResponse([Description("The text the user typed in with their query")] string input)
    {
        string prompt = @$"{SystemText}

Here is a sample chat transcript:

Bot: How can I help you?
User: {input}

---------------------------------------------

Bot: ";
        return await _kernel.GetPromptedReplyAsync(prompt);
    }

    [SKFunction, Description("Sends a raw prompt to a large language model and returns the response")]
    public async Task<string> GetPromptResponse([Description("The prompt for the large language model")] string prompt)
    {
        return await _kernel.GetPromptedReplyAsync($"{SystemText} {prompt}");
    }

    [SKFunction, Description("Displays a response to the user")]
    public string DisplayResponse([Description("The response to show to the user")] string response) => response;
}
