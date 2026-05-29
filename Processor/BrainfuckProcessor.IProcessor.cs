using Esolang.Processor;

namespace Esolang.Brainfuck.Processor;

public sealed partial class BrainfuckProcessor : IProcessor<ReadOnlyMemory<BrainfuckSequence>>
{
    ReadOnlyMemory<BrainfuckSequence> IProcessor<ReadOnlyMemory<BrainfuckSequence>>.Program => Program;
}
