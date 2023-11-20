using Microsoft.SemanticKernel;
using System.ComponentModel;
using Microsoft.SemanticKernel.AI.ChatCompletion;
using Microsoft.SemanticKernel.Orchestration;

namespace MattEland.BatComputer.Kernel.Plugins;

public class ChatPlugin
{
    [SKFunction, Description("Sends a question to a large language model as part of a chat traanscript")]
    public async Task<string> GetChatResponse([Description("The text the user typed in with their query")] string input, SKContext context)
    {
        return await GetPromptedReply(input, context);
    }

    private static async Task<string> GetPromptedReply(string prompt, SKContext context)
    {
        IChatCompletion? chatService = context.ServiceProvider.GetService<IChatCompletion>();
        if (chatService == null)
        {
            return "No chat completion service is configured.";
        }

        IReadOnlyList<IChatResult> completions = await chatService.GetChatCompletionsAsync(chatService.CreateNewChat(prompt));

        if (!completions.Any())
        {
            return "No chat response was returned";
        }

        ChatMessage chatMessage = await completions.First().GetChatMessageAsync();

        return chatMessage.Content;
    }

    [SKFunction, Description("Displays a response to the user")]
    public string DisplayResponse([Description("The response to show to the user")] string response) => response;
}
