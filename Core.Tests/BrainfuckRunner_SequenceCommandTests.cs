using Brainfuck.Core.SequenceCommands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using Command = Brainfuck.BrainfuckRunner.SequenceCommand;

namespace Brainfuck.Tests;

[TestClass]
public class BrainfuckRunner_SequenceCommandTests
{
    public TestContext TestContext { get; set; } = default!;
    [TestMethod]
    public void SequenceCommandTest()
    {
        var innerCommand = new CommentCommand(new(Sequences: new[] { BrainfuckSequence.Comment }.AsMemory(), Stack: ImmutableArray.Create<byte>(0)));
        var command = new Command(innerCommand);
        var (command2, _) = command;
        Assert.AreEqual(innerCommand, command2);
    }
    public void RequiredInputTest()
    {

        var innerCommand = new CommentCommand(new(Sequences: new[] { BrainfuckSequence.Comment }.AsMemory(), Stack: ImmutableArray.Create<byte>(0)));
        var command = new Command(innerCommand);
        Assert.AreEqual(innerCommand.RequiredInput, command.RequiredInput);
    }
    public void RequiredOutputTest()
    {

        var innerCommand = new CommentCommand(new(Sequences: new[] { BrainfuckSequence.Comment }.AsMemory(), Stack: ImmutableArray.Create<byte>(0)));
        var command = new Command(innerCommand);
        Assert.AreEqual(innerCommand.RequiredOutput, command.RequiredOutput);
    }
    public void ToStringTest()
    {

        var innerCommand = new CommentCommand(new(Sequences: new[] { BrainfuckSequence.Comment }.AsMemory(), Stack: ImmutableArray.Create<byte>(0)));
        var command = new Command(innerCommand);
        var str = command.ToString();
        TestContext.WriteLine(str);
        Assert.IsNotNull(str);
    }
}
