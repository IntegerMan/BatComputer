using Microsoft.SemanticKernel.Memory;

namespace MattEland.BatComputer.Kernel.FileMemoryStore;

public record class ScoredMemoryRecord(MemoryRecord Record, double Score)
{
}