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
            // InternalStepCommands が内部的に context を更新しているため、ここではその結果を利用する
            // SequenceCommand の中身を見て IOEvent を決定する。

            if (command is InputCommand inputCommand)
            {
                var inputEvent = new InputCharEventImpl();
                yield return inputEvent;
                var inputChar = await inputEvent.Task;
                
                // 命令の実行とcontextの更新
                // InputCommand.ExecuteAsync は内部で TryInput を呼んでいる
                // ここで実際に値を読み込むロジックを再構築する必要がある
                context = await inputCommand.ExecuteAsync(cancellationToken); 
            }
            else if (command is OutputCommand outputCommand)
            {
                yield return new OutputCharEvent((char)context.Stack[context.StackIndex]);
                context = await outputCommand.ExecuteAsync(cancellationToken);
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
