using Microsoft.SemanticKernel.Memory;

namespace MattEland.BatComputer.Kernel.FileMemoryStore;

public class MemoryRecordCollection
{
    public string Collection { get; set; } = string.Empty;
    public List<MemoryRecord> Records { get; set; } = new();
}
