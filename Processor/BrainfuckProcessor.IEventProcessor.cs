using Esolang.Processor;
using Esolang.Brainfuck.Processor.SequenceCommands;
using System.Collections.Immutable;

namespace Esolang.Brainfuck.Processor;

public sealed partial class BrainfuckProcessor : IEventProcessor
{
    private sealed class InputCharEventImpl : InputCharEvent
    {
        private readonly TaskCompletionSource<char> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override void Write(char c) => _tcs.TrySetResult(c);
        public Task<char> Task => _tcs.Task;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<IOEvent> RunAsyncEnumerable([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var context = Context;
        
        while (BrainfuckSequenceCommand.TryGetCommand(context, out var command))
        {
            // BrainfuckSequenceCommand自体を直接判定する
            
            if (command is InputCommand)
            {
                var inputEvent = new InputCharEventImpl();
                yield return inputEvent;
                var inputChar = await inputEvent.Task;
                
                context = context with {
                    Stack = context.Stack.SetItem(context.StackIndex, (byte)inputChar),
                    SequencesIndex = context.SequencesIndex + 1
                };
            }
            else if (command is OutputCommand)
            {
                yield return new OutputCharEvent((char)context.Stack[context.StackIndex]);
                context = context with {
                    SequencesIndex = context.SequencesIndex + 1
                };
            }
            else
            {
                // その他の命令は既存の Execute ロジックで実行
                context = await command.ExecuteAsync(cancellationToken);
            }
        }
        
        yield return new EndEvent(0);
    }
}
