using Microsoft.SemanticKernel.Memory;

namespace MattEland.BatComputer.Memory.FileMemoryStore;

public class MemoryRecordCollection
{
    public string Collection { get; set; } = string.Empty;
    public List<MemoryRecord> Records { get; set; } = new();
}
