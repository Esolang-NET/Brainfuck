namespace Esolang.Brainfuck.Generator.Tests;

public class ParameterOptionsTests
{
    [Test]
    public async Task ParameterOptions_HasProperties_CorrectBehavior()
    {
        var options = new ParameterOptions(
            ParameterSymbols: "",
            VariableCancellation: null,
            VariablePipeWriter: "writer",
            VariableTextWriter: null,
            VariablePipeReader: "reader",
            VariableTextReader: null,
            VariableInputString: "input",
            VariableLogger: null,
            IsLoggerFromParameter: false
        );

        await Assert.That(options.HasPipeWriterParameter).IsTrue();
        await Assert.That(options.HasTextWriterParameter).IsFalse();
        await Assert.That(options.HasPipeReaderParameter).IsTrue();
        await Assert.That(options.HasTextReaderParameter).IsFalse();
        await Assert.That(options.HasInputStringParameter).IsTrue();
    }
}
