namespace MattEland.BatComputer.Abstractions.Widgets;

public class TokenUsageWidget : WidgetBase
{
    public TokenUsageWidget() : this(1, 1)
    {
    }

    public TokenUsageWidget(int promptTokens, int completionTokens)
    {
        Title = $"{promptTokens + completionTokens} Tokens Used";
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
