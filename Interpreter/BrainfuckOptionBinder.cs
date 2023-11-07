using System.CommandLine;
using System.CommandLine.Binding;
namespace Esolang.Brainfuck.Interpreter;

/// <summary>
/// brainfuck global options
/// </summary>
public class BrainfuckOptionBinder : BinderBase<IBrainfuckOptions>
{
    /// <summary>
    /// brainfuck global options
    /// </summary>
    /// <param name="noUseDefaultValue"></param>
    /// <param name="incrementPointer"></param>
    /// <param name="decrementPointer"></param>
    /// <param name="incrementCurrent"></param>
    /// <param name="decrementCurrent"></param>
    /// <param name="output"></param>
    /// <param name="input"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    public BrainfuckOptionBinder(
        Option<bool> noUseDefaultValue,
        Option<string?> incrementPointer,
        Option<string?> decrementPointer,
        Option<string?> incrementCurrent,
        Option<string?> decrementCurrent,
        Option<string?> output,
        Option<string?> input,
        Option<string?> begin,
        Option<string?> end)
        => (NoUseDefaultValue, IncrementPointer, DecrementPointer, IncrementCurrent, DecrementCurrent, Output, Input, Begin, End) = (noUseDefaultValue, incrementPointer, decrementPointer, incrementCurrent, decrementCurrent, output, input, begin, end);
    /// <summary>
    /// deconstract brainfuck global options
    /// </summary>
    /// <param name="noUseDefaultValue"></param>
    /// <param name="incrementPointer"></param>
    /// <param name="decrementPointer"></param>
    /// <param name="incrementCurrent"></param>
    /// <param name="decrementCurrent"></param>
    /// <param name="output"></param>
    /// <param name="input"></param>
    /// <param name="begin"></param>
    /// <param name="end"></param>
    public void Deconstruct(
        out Option<bool> noUseDefaultValue,
        out Option<string?> incrementPointer,
        out Option<string?> decrementPointer,
        out Option<string?> incrementCurrent,
        out Option<string?> decrementCurrent,
        out Option<string?> output,
        out Option<string?> input,
        out Option<string?> begin,
        out Option<string?> end)
        => (noUseDefaultValue, incrementPointer, decrementPointer, incrementCurrent, decrementCurrent, output, input, begin, end) = (NoUseDefaultValue, IncrementPointer, DecrementPointer, IncrementCurrent, DecrementCurrent, Output, Input, Begin, End);
    /// <summary>
    /// internal brainfuck options model
    /// </summary>
    /// <param name="IncrementPointer"></param>
    /// <param name="DecrementPointer"></param>
    /// <param name="IncrementCurrent"></param>
    /// <param name="DecrementCurrent"></param>
    /// <param name="Output"></param>
    /// <param name="Input"></param>
    /// <param name="Begin"></param>
    /// <param name="End"></param>
    record BrainfuckOptions(string IncrementPointer, string DecrementPointer, string IncrementCurrent, string DecrementCurrent, string Output, string Input, string Begin, string End) : IBrainfuckOptions;
    /// <summary>
    /// no use default value.
    /// </summary>
    readonly Option<bool> NoUseDefaultValue;
    /// <summary>
    /// Increment the data pointer by one (to point to the next cell to the right).
    /// </summary>
    readonly Option<string?> IncrementPointer;
    /// <summary>
    /// Decrement the data pointer by one (to point to the next cell to the left).
    /// </summary>
    readonly Option<string?> DecrementPointer;
    /// <summary>
    /// Increment the byte at the data pointer by one.
    /// </summary>
    readonly Option<string?> IncrementCurrent;
    /// <summary>
    /// Decrement the byte at the data pointer by one.
    /// </summary>
    readonly Option<string?> DecrementCurrent;
    /// <summary>
    /// Output the byte at the data pointer.
    /// </summary>
    readonly Option<string?> Output;
    /// <summary>
    /// Accept one byte of input, storing its value in the byte at the data pointer.
    /// </summary>
    readonly Option<string?> Input;
    /// <summary>
    /// If the byte at the data pointer is zero, then instead of moving the instruction pointer forward to the next command, jump it forward to the command after the matching ] command.
    /// </summary>
    readonly Option<string?> Begin;
    /// <summary>
    /// If the byte at the data pointer is nonzero, then instead of moving the instruction pointer forward to the next command, jump it back to the command after the matching [ command.
    /// </summary>
    readonly Option<string?> End;
    protected override IBrainfuckOptions GetBoundValue(BindingContext bindingContext)
    {
        var noUseDefaultValue = bindingContext.ParseResult.GetValueForOption(NoUseDefaultValue);
        var incrementPointer = bindingContext.ParseResult.GetValueForOption(IncrementPointer);
        if (!noUseDefaultValue && string.IsNullOrEmpty(incrementPointer))
            incrementPointer = BrainfuckOptionsDefault.IncrementPointer;
        var decrementPointer = bindingContext.ParseResult.GetValueForOption(DecrementPointer);
        if (!noUseDefaultValue && string.IsNullOrEmpty(decrementPointer))
            decrementPointer = BrainfuckOptionsDefault.DecrementPointer;
        var incrementCurrent = bindingContext.ParseResult.GetValueForOption(IncrementCurrent);
        if (!noUseDefaultValue && string.IsNullOrEmpty(incrementCurrent))
            incrementCurrent = BrainfuckOptionsDefault.IncrementCurrent;
        var decrementCurrent = bindingContext.ParseResult.GetValueForOption(DecrementCurrent);
        if (!noUseDefaultValue && string.IsNullOrEmpty(decrementCurrent))
            decrementCurrent = BrainfuckOptionsDefault.DecrementCurrent;
        var output = bindingContext.ParseResult.GetValueForOption(Output);
        if (!noUseDefaultValue && string.IsNullOrEmpty(output))
            output = BrainfuckOptionsDefault.Output;
        var input = bindingContext.ParseResult.GetValueForOption(Input);
        if (!noUseDefaultValue && string.IsNullOrEmpty(input))
            input = BrainfuckOptionsDefault.Input;
        var begin = bindingContext.ParseResult.GetValueForOption(Begin);
        if (!noUseDefaultValue && string.IsNullOrEmpty(begin))
            begin = BrainfuckOptionsDefault.Begin;
        var end = bindingContext.ParseResult.GetValueForOption(End);
        if (!noUseDefaultValue && string.IsNullOrEmpty(end))
            end = BrainfuckOptionsDefault.End;
        return new BrainfuckOptions(
            IncrementPointer: incrementPointer!,
            DecrementPointer: decrementPointer!,
            IncrementCurrent: incrementCurrent!,
            DecrementCurrent: decrementCurrent!,
            Output: output!,
            Input: input!,
            Begin: begin!,
            End: end!
        );
    }
}
