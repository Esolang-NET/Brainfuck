namespace Esolang.Brainfuck.Generator.Tests;

[TestClass]
public class ParameterOptionsTests
{
    [TestMethod]
    public void ParameterOptions_HasProperties_CorrectBehavior()
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

        Assert.IsTrue(options.HasPipeWriterParameter);
        Assert.IsFalse(options.HasTextWriterParameter);
        Assert.IsTrue(options.HasPipeReaderParameter);
        Assert.IsFalse(options.HasTextReaderParameter);
        Assert.IsTrue(options.HasInputStringParameter);
    }
}
