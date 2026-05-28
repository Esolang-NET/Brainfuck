using Esolang.Brainfuck.Processor.SequenceCommands;
using Esolang.Processor;
using System.Collections.Immutable;

namespace Esolang.Brainfuck.Processor;

public sealed partial class BrainfuckProcessor : IEventProcessor
{
    /// <inheritdoc/>
    public async IAsyncEnumerable<IOEvent> RunAsyncEnumerable([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var context = Context;

        while (BrainfuckSequenceCommand.TryGetCommand(context, out var command))
        {
            if (command.IsIoCommand)
            {
                var ioEvent = await command.GetIoEventAsync(cancellationToken);
                if (ioEvent is not null)
                {
                    yield return ioEvent;
                    context = await command.ExecuteAsync(ioEvent, cancellationToken);
                }
                else
                {
                    context = await command.ExecuteAsync(cancellationToken);
                }
            }
            else
            {
                context = await command.ExecuteAsync(cancellationToken);
            }
        }

        yield return new EndEvent(0);
    }
}
