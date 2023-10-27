// See https://aka.ms/new-console-template for more information
using Brainfuck;

Console.WriteLine("Hello, World!");

Console.WriteLine($"{nameof(BrainfuckSample.SampleMethod)}: {await BrainfuckSample.SampleMethod()}");

partial class BrainfuckSample
{
    [GenerateBrainfuckMethod("+++++++++[>++++++++>+++++++++++>+++++<<<-]>.>++.+++++++..+++.>-.------------.<++++++++.--------.+++.------.--------.>+.")]
    public static partial Task<string> SampleMethod();
}
