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

namespace MattEland.BatComputer.Kernel;

public class ChatPlugin
{
    private const string SystemText = "You are an AI assistant named Alfred, the virtual butler to Batman. The user is Batman.";

    private readonly IKernel _kernel;
    private readonly ISKFunction _chatFunc;
    //private readonly ISKFunction _webFunc;

    public ChatPlugin(IKernel kernel)
    {
        _kernel = kernel;

        //ChatRequestSettings llamaSettings = new(); 
        //OpenAIRequestSettings requestSettings = new() { ChatSystemPrompt = SystemText, ResultsPerPrompt = 1 };

        string chatPrompt = @$"{SystemText}

Here is a sample chat transcript:

Bot: How can I help you?
User: {{$input}}

---------------------------------------------

Bot: ";
        _chatFunc = kernel.CreateSemanticFunction(chatPrompt, "Chat", "SemanticFunctions");

        /*
        string webPrompt = "Make a GET request to {{$input}} and describe the text of that site.";
        _webFunc = kernel.CreateSemanticFunction(webPrompt, requestSettings, "WebSummarize", "SemanticFunctions");
        */
    }

    [SKFunction, Description("Gets a response to a question")]
    public async Task<string> GetChatResponse([Description("The text the user typed in with their query")] string input)
    {
        KernelResult result = await _kernel.RunAsync(input, _chatFunc);
        return result.GetValue<string>() ?? "";
    }

    /*
    [SKFunction, Description("Summarizes the contents of a web page"), UsedImplicitly]
    public async Task<string> InterpretWebPage([Description("A web page URL the user is interested in")] string url)
    {
        KernelResult result = await _kernel.RunAsync(url, _webFunc);
        return result.GetValue<string>() ?? "";
    }
    */
}
