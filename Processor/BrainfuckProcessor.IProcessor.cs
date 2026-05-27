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

    private sealed class InputIntEventImpl : InputIntEvent
    {
        private readonly TaskCompletionSource<int> _tcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public override void Write(int i) => _tcs.TrySetResult(i);
        public Task<int> Task => _tcs.Task;
    }

    /// <inheritdoc/>
    public async IAsyncEnumerable<IOEvent> RunAsyncEnumerable([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var context = Context;
        
        while (BrainfuckSequenceCommand.TryGetCommand(context, out var command))
        {
            if (command is InputCommand)
            {
                var inputEvent = new InputCharEventImpl();
                yield return inputEvent;
                var inputChar = await inputEvent.Task; // cancellationToken の処理を別途考える必要があります
                context = context with {
                    Stack = context.Stack.SetItem(context.StackIndex, (byte)inputChar),
                    SequencesIndex = context.SequencesIndex + 1
                };
            }
            else if (command is OutputCommand outputCommand)
            {
                yield return new OutputCharEvent((char)context.Stack[context.StackIndex]);
                context = context with { SequencesIndex = context.SequencesIndex + 1 };
            }
            else
            {
                // その他の命令は既存の Execute ロジックで実行
                context = command.Execute(cancellationToken);
            }
        }
        
        yield return new EndEvent(0);
    }
}
