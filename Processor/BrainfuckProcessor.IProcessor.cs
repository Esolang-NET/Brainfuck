using Esolang.Processor;

namespace Esolang.Brainfuck.Processor;

public sealed partial class BrainfuckProcessor : IEventProcessor, IProcessor<ReadOnlyMemory<BrainfuckSequence>>
{
    /// <inheritdoc/>
    ReadOnlyMemory<BrainfuckSequence> IProcessor<ReadOnlyMemory<BrainfuckSequence>>.Program => Program;

    /// <inheritdoc/>
    public async IAsyncEnumerable<IOEvent> RunAsyncEnumerable([System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var context = Context;
        // InternalStepCommands を利用して、現在の命令列を順次実行し、IOEventを生成します。
        // ここで command.ExecuteAsync や Execute が IOEvent を返す形に変換する必要があります。
        
        // 現状の BrainfuckProcessor は InternalStepCommands で Sequential に実行していますが、
        // これを IOEvent を yield する形式に書き換えます。
        
        await Task.CompletedTask;
        yield return new EndEvent(0);
    }
}
