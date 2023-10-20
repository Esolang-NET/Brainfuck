﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using static Brainfuck.BrainfuckSequence;
using Command = Brainfuck.Core.SequenceCommands.IncrementCurrentCommand;

namespace Brainfuck.Core.SequenceCommands.Tests;

[TestClass()]
public class IncrementCurrentCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    static IEnumerable<object?[]> ExecuteTestData
    {
        get
        {
            {
                // currentStack +1
                var sequences = new[] { IncrementCurrent }.AsMemory();
                var stack = ImmutableArray.Create<byte>(0);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack
                );
                yield return ExecuteAsyncTest(
                    context,
                    context with
                    {
                        Stack = ImmutableArray.Create<byte>(1),
                        SequencesIndex = 1,
                    }
                );
            }
            {
                // stackPointer +1 overflow 255 → 0
                var sequences = new[] { IncrementCurrent }.AsMemory();
                var stack = ImmutableArray.Create(byte.MaxValue);
                BrainfuckContext context = new(
                    Sequences: sequences,
                    Stack: stack
                );
                yield return ExecuteAsyncTest(
                    context,
                    context with
                    {
                        Stack = ImmutableArray.Create(byte.MinValue),
                        SequencesIndex = 1,
                    }
                );
            }
            static object?[] ExecuteAsyncTest(BrainfuckContext context, BrainfuckContext expected)
                => new object?[] { context, expected };
        }
    }
    [TestMethod]
    [DynamicData(nameof(ExecuteTestData))]
    public async Task ExecuteAsyncTest(BrainfuckContext context, BrainfuckContext expected)
    {
        var token = TestContext.CancellationTokenSource.Token;

        var actual = await new Command(context).ExecuteAsync(token);
        Assert.AreEqual(expected, actual);
    }
    [TestMethod]
    [DynamicData(nameof(ExecuteTestData))]
    public void ExecuteTest(BrainfuckContext context, BrainfuckContext expected)
    {
        var actual = new Command(context).Execute();
        Assert.AreEqual(expected, actual);
    }
    [TestMethod]
    public void RequiredInputTest()
    {
        var command = new Command(default);
        Assert.AreEqual(false, command.RequiredInput);
    }
    [TestMethod]
    public void RequiredOutputTest()
    {
        var command = new Command(default);
        Assert.AreEqual(false, command.RequiredOutput);
    }
}