using Microsoft.SemanticKernel.Memory;

namespace MattEland.BatComputer.Abstractions.Widgets;
public class MemoryQueryResultWidget : WidgetBase
{
    public MemoryQueryResultWidget()
    {
        Title = "Memory Query Result";
        Id = "Not Set";
        Text = null;
        Description = null;
    }

    public MemoryQueryResultWidget(string collection, MemoryQueryResult result) : this()
    {
        Title = $"{collection}:{result.Metadata.Id}";
        Id = result.Metadata.Id;
        Text = result.Metadata.Text;
        Description = result.Metadata.Description;
        ExternalSourceName = result.Metadata.ExternalSourceName;
        Relevance = result.Relevance;
    }

    public string Id { get; set; }
    public string? Text { get; set; }
    public string? Description { get; set; }
    public string? ExternalSourceName { get; set; }
    public double Relevance { get; set; }

    public override void UseSampleData()
    {
        Id = "SAMP-13";
        Text = "Forty Two";
        Description = "The meaning of life, the universe, and everything";
        ExternalSourceName = "H2G2";
        Relevance = 0.42;
    }
}
