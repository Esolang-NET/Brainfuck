// See https://aka.ms/new-console-template for more information
using Brainfuck;

Console.WriteLine("Hello, World!");


partial class BrainfuckSample
{
    [GenerateBrainfuckMethod("++++++++++++++++++++++++++++++++++++++++\r\n+++++++++++++++++++++++++.+.+.>++++++++++")]
    public static partial Task<string> SampleMethod(string input, CancellationToken cancellation);
}
