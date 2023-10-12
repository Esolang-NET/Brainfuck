using Microsoft.VisualStudio.TestTools.UnitTesting;

/* プロジェクト 'Brainfuck.Core.Tests (net48)' からのマージされていない変更
前:
using Brainfuck;
using System;
後:
using System;
using System.Collections.Generic;
*/
using System.IO.Pipelines;
using System.Text;
/* プロジェクト 'Brainfuck.Core.Tests (net48)' からのマージされていない変更
前:
using System.Threading.Tasks;
using System.IO.Pipelines;
後:
using System.Threading.Tasks;
*/


namespace Brainfuck.Tests;

[TestClass()]
public class BrainfuckRunnerTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object?[]> RunAndOutputStringAsyncTestData
    {
        get
        {
            yield return RunAndOutputStringAsyncTest(
                source: "+++++++++[>++++++++>+++++++++++>+++++<<<-]>.>++.+++++++..+++.>-.------------.<++++++++.--------.+++.------.--------.>+.",
                expected: "Hello, world!"
            );
            yield return RunAndOutputStringAsyncTest(
                source: "+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++.+.+.>++++++++++.",
                expected: "ABC\n"
            );
            yield return RunAndOutputStringAsyncTest(
                source: "++++++[>++++++++<-]++++++++++[>.+<-]",
                expected: "0123456789"
            );

            static object?[] RunAndOutputStringAsyncTest(string source, string? input = default, string? expected = default)
                => new object?[] { source, input, expected };
        }
    }
    [TestMethod]
    [DynamicData(nameof(RunAndOutputStringAsyncTestData))]
    public async Task RunAndOutputStringAsyncTest(string source, string? input, string? expected = default)
    {
        var token = TestContext.CancellationTokenSource.Token;
        var pipe = new Pipe();
        var enumerable = new BrainfuckSequencer(source);
        var sequences = enumerable.Select(v => v.Sequence).ToArray().AsMemory();
        var runner = new BrainfuckRunner(sequences, input: pipe.Reader);
        var awaiter = runner.RunAndOutputStringAsync(token);
        if (!string.IsNullOrEmpty(input))
            await pipe.Writer.WriteAsync(Encoding.UTF8.GetBytes(input), token);
        var actual = await awaiter;
        Assert.AreEqual(expected, actual);
    }
}