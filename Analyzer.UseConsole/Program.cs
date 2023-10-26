// See https://aka.ms/new-console-template for more information
using Brainfuck;

Console.WriteLine("Hello, World!");

Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod)}: {await BrainfuckSample.SampleMethod("123", default)}");
Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod2)}: {BrainfuckSample.SampleMethod2()}");

partial class BrainfuckSample
{
    [GenerateBrainfuckMethod("++++++++++++++++++++++++++++++++++++++++\r\n+++++++++++++++++++++++++.+.+.>++++++++++")]
    public static partial Task<string> SampleMethod(string input, CancellationToken cancellation = default);
    [GenerateBrainfuckMethod("++++++++++++++++++++++++++++++++++++.")]
    public static partial string SampleMethod2();
}
