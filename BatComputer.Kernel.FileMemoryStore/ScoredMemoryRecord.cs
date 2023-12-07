using Microsoft.SemanticKernel.Memory;

namespace MattEland.BatComputer.Memory.FileMemoryStore;

public record class ScoredMemoryRecord(MemoryRecord Record, double Score)
{
}