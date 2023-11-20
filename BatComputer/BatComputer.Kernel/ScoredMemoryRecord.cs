using Microsoft.SemanticKernel.Memory;

namespace MattEland.BatComputer.Kernel;

public record class ScoredMemoryRecord(MemoryRecord Record, double Score)
{
}