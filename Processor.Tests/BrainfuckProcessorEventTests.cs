using Esolang.Processor;

namespace Esolang.Brainfuck.Processor.Tests;

[TestClass]
public class BrainfuckProcessorEventTests
{
    [TestMethod]
    public async Task RunAsyncEnumerable_ProducesOutputEvents()
    {
        // "+" して "." するプログラム
        var processor = new BrainfuckProcessor("+.");
        var events = new List<IOEvent>();

        await foreach (var ev in processor.RunAsyncEnumerable(TestContext.CancellationToken))
        {
            events.Add(ev);
        }

        Assert.HasCount(2, events); // OutputCharEvent, EndEvent
        Assert.IsInstanceOfType<OutputCharEvent>(events[0]);
        Assert.AreEqual(1, ((OutputCharEvent)events[0]).Output);
        Assert.IsInstanceOfType<EndEvent>(events[1]);
    }

    [TestMethod]
    public async Task RunAsyncEnumerable_HandlesInputEvent()
    {
        // "," して "." するプログラム
        var processor = new BrainfuckProcessor(",.");
        var events = new List<IOEvent>();

        var enumerator = processor.RunAsyncEnumerable(TestContext.CancellationToken).GetAsyncEnumerator(TestContext.CancellationToken);

        // Input 命令に到達
        Assert.IsTrue(await enumerator.MoveNextAsync());
        Assert.IsInstanceOfType<InputCharEvent>(enumerator.Current);

        // 入力を提供
        ((InputCharEvent)enumerator.Current).Write('A');

        // 次の命令（出力）に到達
        Assert.IsTrue(await enumerator.MoveNextAsync());
        Assert.IsInstanceOfType<OutputCharEvent>(enumerator.Current);
        Assert.AreEqual('A', ((OutputCharEvent)enumerator.Current).Output);

        // EndEvent
        Assert.IsTrue(await enumerator.MoveNextAsync());
        Assert.IsInstanceOfType<EndEvent>(enumerator.Current);
    }

    public TestContext TestContext { get; set; }
}
