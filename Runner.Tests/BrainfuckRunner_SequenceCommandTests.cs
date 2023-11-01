using Brainfuck.Runner.SequenceCommands;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Immutable;
using Command = Brainfuck.Runner.BrainfuckRunner.SequenceCommand;

namespace Brainfuck.Runner.Tests;

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
    [TestMethod]
    public void SequenceCommand_ThrowIfArgumentNullTest()
        => Assert.ThrowsException<ArgumentNullException>(() => new Command(default!));
    [TestMethod]
    public void SequenceCommand_IfAgumentIsSequenceCommandTest()
    {
        var innerCommand = new CommentCommand(new(Sequences: new[] { BrainfuckSequence.Comment }.AsMemory(), Stack: ImmutableArray.Create<byte>(0)));
        var command1 = new Command(innerCommand);
        var command2 = new Command(command1);
        var (command1_inner, _) = command1;
        var (command2_inner, _) = command2;
        Assert.AreEqual(innerCommand, command2_inner);
        Assert.AreEqual(command1_inner, command2_inner);
    }
    [TestMethod]
    public void RequiredInputTest()
    {

        var innerCommand = new CommentCommand(new(Sequences: new[] { BrainfuckSequence.Comment }.AsMemory(), Stack: ImmutableArray.Create<byte>(0)));
        var command = new Command(innerCommand);
        Assert.AreEqual(innerCommand.RequiredInput, command.RequiredInput);
    }
    [TestMethod]
    public void RequiredOutputTest()
    {

        var innerCommand = new CommentCommand(new(Sequences: new[] { BrainfuckSequence.Comment }.AsMemory(), Stack: ImmutableArray.Create<byte>(0)));
        var command = new Command(innerCommand);
        Assert.AreEqual(innerCommand.RequiredOutput, command.RequiredOutput);
    }
    [TestMethod]
    public void ToStringTest()
    {

        var innerCommand = new CommentCommand(new(Sequences: new[] { BrainfuckSequence.Comment }.AsMemory(), Stack: ImmutableArray.Create<byte>(0)));
        var command = new Command(innerCommand);
        var str = command.ToString();
        TestContext.WriteLine(str);
        Assert.IsNotNull(str);
    }
}
