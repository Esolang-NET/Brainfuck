// See https://aka.ms/new-console-template for more information
using Brainfuck;

Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod1)}: {await BrainfuckSample.SampleMethod1()}");
Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod2)}: {await BrainfuckSample.SampleMethod2()}");

partial class BrainfuckSample
{
    [GenerateBrainfuckMethod("++++++[>++++++++<-]++++++++++[>.+<-]")]
    public static partial Task<string?> SampleMethod1();
    [GenerateBrainfuckMethod("1+++++++++[>++++++++>+++++++++++>+++++<<<-]>.>++.+++++++..+++.>-.------------.<++++++++.--------.+++.------.--------.>+.")]
    public static partial Task<string?> SampleMethod2();

    [GenerateBrainfuckMethod("😀😁😂🤣😃😄😅😅😅😅😆😆", IncrementPointer = "😀", DecrementPointer = "😁", IncrementCurrent = "😂", DecrementCurrent = "🤣", Output = "😃", Input = "😄", Begin = "😅", End = "😆")]
    public static partial Task<string?> SampleMethod(string input);
}
