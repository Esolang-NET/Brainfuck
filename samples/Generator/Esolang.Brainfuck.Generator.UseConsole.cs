#!/usr/bin/env dotnet run 
#:sdk Microsoft.NET.Sdk 
#:property TargetFramework=net10.0 
#:property TargetFrameworks=net8.0;net9.0;net10.0 
#:property IsPackable=false 
#:property OutputType=Exe 
#:property IsPublishable=false 
#:property IsTestProject=false

using Esolang.Brainfuck;

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
