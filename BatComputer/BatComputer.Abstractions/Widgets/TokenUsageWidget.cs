namespace MattEland.BatComputer.Abstractions.Widgets;

public class TokenUsageWidget : WidgetBase
{
    public TokenUsageWidget() : this(1, 1)
    {
    }

    public TokenUsageWidget(int promptTokens, int completionTokens, string title = "")
    {
        Title = $"{title}: {promptTokens + completionTokens} Tokens Used".TrimStart();
        PromptTokens = promptTokens;
        CompletionTokens = completionTokens;
    }

    public int CompletionTokens { get; set; }

    public int PromptTokens { get; set; }

    public override void UseSampleData()
    {
        Title = "Plan: 260 Tokens Used";
        PromptTokens = 200;
        CompletionTokens = 60;
    }
}
