using Microsoft.SemanticKernel;
using System.ComponentModel;
using LLamaSharp.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Orchestration;

namespace MattEland.BatComputer.Kernel;

public class ChatPlugin
{
    private readonly AppKernel _kernel;

    public ChatPlugin(AppKernel kernel)
    {
        _kernel = kernel;
    }

    [SKFunction, Description("Sends a question to a large language model as part of a chat traanscript")]
    public async Task<string> GetChatResponse([Description("The text the user typed in with their query")] string input, SKContext context)
    {
        string prompt = @$"{_kernel.SystemText}

Here is a sample chat transcript:

Bot: How can I help you?
User: {input}

---------------------------------------------

Bot: ";
        IChatCompletion? chatService = context.ServiceProvider.GetService<IChatCompletion>();
        if (chatService == null)
        {
            return "No chat completion service is configured.";
        }

        ChatRequestSettings settings = new() { ResultsPerPrompt = 1};
        IReadOnlyList<IChatResult> completions =
            await chatService.GetChatCompletionsAsync(chatService.CreateNewChat(prompt), settings);

        if (!completions.Any())
        {
            return "No chat response was returned";
        }

        ChatMessage chatMessage = await completions.First().GetChatMessageAsync();

        return chatMessage.Content;
    }
}
