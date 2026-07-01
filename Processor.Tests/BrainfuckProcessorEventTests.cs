using Esolang.Processor;
using static Esolang.Processor.IOEvent;

namespace Esolang.Brainfuck.Processor.Tests;

public class BrainfuckProcessorEventTests
{
    readonly TestContext TestContext;
    public BrainfuckProcessorEventTests() => TestContext = TestContext.Current!;

    [Test]
    public async Task RunAsyncEnumerable_ProducesOutputEvents(CancellationToken CancellationToken)
    {
        // "+" して "." するプログラム
        var processor = new BrainfuckProcessor("+.");
        var events = new List<IOEvent>();

        await foreach (var ev in processor.RunAsyncEnumerable(CancellationToken))
        {
            events.Add(ev);
        }

        await Assert.That(events).Count().IsEqualTo(2); // OutputCharEvent, EndEvent
        await Assert.That(events[0]).IsTypeOf<OutputCharEvent>()
            .And.HasProperty(v => v.Output).IsEqualTo((char)1);
        await Assert.That(events[1]).IsTypeOf<EndEvent>();
    }

    [Test]
    public async Task RunAsyncEnumerable_HandlesInputEvent(CancellationToken CancellationToken)
    {
        // "," して "." するプログラム
        var processor = new BrainfuckProcessor(",.");
        var events = new List<IOEvent>();

        var enumerator = processor.RunAsyncEnumerable(CancellationToken).GetAsyncEnumerator(CancellationToken);

        // Input 命令に到達
        await Assert.That(await enumerator.MoveNextAsync()).IsTrue();
        await Assert.That(enumerator.Current).IsTypeOf<InputCharEvent>();

        // 入力を提供
        ((InputCharEvent)enumerator.Current).Write('A');

        // 次の命令（出力）に到達
        await Assert.That(await enumerator.MoveNextAsync()).IsTrue();
        await Assert.That(enumerator.Current).IsTypeOf<OutputCharEvent>()
            .And.HasProperty(v => v.Output).IsEqualTo('A');

        // EndEvent
        await Assert.That(await enumerator.MoveNextAsync()).IsTrue();
        await Assert.That(enumerator.Current).IsTypeOf<EndEvent>();
    }
}
