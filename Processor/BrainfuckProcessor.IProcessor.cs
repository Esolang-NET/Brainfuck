using Esolang.Processor;
using Esolang.Brainfuck.Processor.SequenceCommands;
using System.Collections.Immutable;

namespace Esolang.Brainfuck.Processor;

public sealed partial class BrainfuckProcessor : IProcessor<ReadOnlyMemory<BrainfuckSequence>>
{
    ReadOnlyMemory<BrainfuckSequence> IProcessor<ReadOnlyMemory<BrainfuckSequence>>.Program => Program;
}
