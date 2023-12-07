using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.ChatCompletion;

namespace MattEland.BatComputer.ConsoleApp.Logging;

public class VerboseLoggingChatCompletion : IChatCompletion, IDisposable
{
    private readonly IChatCompletion _nestedService;
    private readonly ILogger<VerboseLoggingChatCompletion> _log;

    public VerboseLoggingChatCompletion(IChatCompletion nestedService, ILoggerFactory logFactory)
    {
        _nestedService = nestedService;
        _log = logFactory.CreateLogger<VerboseLoggingChatCompletion>();
    }

    public IReadOnlyDictionary<string, string> Attributes => _nestedService.Attributes;

    public ChatHistory CreateNewChat(string? instructions = null)
    {
        _log.LogTrace($"Creating new chat with instructions: {instructions}");
        return _nestedService.CreateNewChat(instructions);
    }

    private readonly HashSet<ChatMessage> _logged = new();

    public Task<IReadOnlyList<IChatResult>> GetChatCompletionsAsync(ChatHistory chat, AIRequestSettings? requestSettings = null, CancellationToken cancellationToken = default)
    {
        _log.LogTrace($"Getting chat completions for chat with new history entries:");
        LogNewMessages(chat);

        return _nestedService.GetChatCompletionsAsync(chat, requestSettings, cancellationToken);
    }

    public IAsyncEnumerable<IChatStreamingResult> GetStreamingChatCompletionsAsync(ChatHistory chat, AIRequestSettings? requestSettings = null, CancellationToken cancellationToken = default)
    {
        _log.LogTrace($"Getting streaming chat completions for chat with new history entries:");
        LogNewMessages(chat);

        return _nestedService.GetStreamingChatCompletionsAsync(chat, requestSettings, cancellationToken);
    }

    private void LogNewMessages(ChatHistory chat)
    {
        foreach (ChatMessage message in chat)
        {
            if (_logged.Add(message))
            {
                _log.LogTrace($"{message.Role}: {message.Content}");
            }
        }
    }

    public void Dispose()
    {
        IDisposable? disposable = _nestedService as IDisposable;
        disposable?.Dispose();
    }
}