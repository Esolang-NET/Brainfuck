using Esolang.Processor;
using System.Diagnostics.CodeAnalysis;

namespace Esolang.Brainfuck.Processor;

public sealed partial class BrainfuckProcessor : IProcessor<ReadOnlyMemory<BrainfuckSequence>>
{
    [ExcludeFromCodeCoverage]
    ReadOnlyMemory<BrainfuckSequence> IProcessor<ReadOnlyMemory<BrainfuckSequence>>.Program => Program;
}
