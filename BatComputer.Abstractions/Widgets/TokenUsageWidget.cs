namespace MattEland.BatComputer.Abstractions.Widgets;

public class TokenUsageWidget : WidgetBase
{
    public TokenUsageWidget() : this(Enumerable.Empty<TokenUsage>())
    {
    }

    public TokenUsageWidget(IEnumerable<TokenUsage> tokens)
    {
        int promptTokens = tokens.Where(t => t.UsageType == TokenUsageType.Prompt).Sum(t => t.TokenCount);
        int completionTokens = tokens.Where(t => t.UsageType == TokenUsageType.Completion).Sum(t => t.TokenCount);

        Title = $"{promptTokens + completionTokens} Tokens Used".TrimStart();
        PromptTokens = promptTokens;
        CompletionTokens = completionTokens;
    }

    public int CompletionTokens { get; set; }

    public int PromptTokens { get; set; }

    public override void UseSampleData()
    {
        Title = "260 Tokens Used";
        PromptTokens = 200;
        CompletionTokens = 60;
    }
}
