// See https://aka.ms/new-console-template for more information
namespace Brainfuck.Console;
/// <summary>
/// brainfuck options.
/// </summary>
public class BrainfuckOptions : IBrainfuckOptions
{
    /// <summary>
    /// brainfuck options.
    /// </summary>
    public BrainfuckOptions() { }

    /// <inheritdoc cref="IBrainfuckOptions.IncrementPointer" />
    public string IncrementPointer { get; set; } = BrainfuckOptionsDefault.IncrementPointer;

    /// <inheritdoc cref="IBrainfuckOptions.DecrementPointer" />
    public string DecrementPointer { get; set; } = BrainfuckOptionsDefault.DecrementPointer;

    /// <inheritdoc cref="IBrainfuckOptions.IncrementCurrent" />
    public string IncrementCurrent { get; set; } = BrainfuckOptionsDefault.IncrementCurrent;

    /// <inheritdoc cref="IBrainfuckOptions.DecrementCurrent" />
    public string DecrementCurrent { get; set; } = BrainfuckOptionsDefault.DecrementCurrent;

    /// <inheritdoc cref="IBrainfuckOptions.Output" />
    public string Output { get; set; } = BrainfuckOptionsDefault.Output;

    /// <inheritdoc cref="IBrainfuckOptions.Input" />
    public string Input { get; set; } = BrainfuckOptionsDefault.Input;

    /// <inheritdoc cref="IBrainfuckOptions.Begin" />
    public string Begin { get; set; } = BrainfuckOptionsDefault.Begin;

    /// <inheritdoc cref="IBrainfuckOptions.End" />
    public string End { get; set; } = BrainfuckOptionsDefault.End;
}
