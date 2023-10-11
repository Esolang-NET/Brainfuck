﻿using Microsoft.CodeAnalysis;

namespace Brainfuck.Analyzer;
[Generator(LanguageNames.CSharp)]
public partial class BrainfuckMethodGenerator : IIncrementalGenerator
{
    public const string CommentAutoGenerated = "// <auto-generated/>";
    public const string NameSpaceName = "Brainfuck";
    public const string ClassNameBrainfuckAttribution = "GenerateBrainfuckMethodAttribute";
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Providerシリーズ
        // context.AdditionalTextsProvider
        // context.AnalyzerConfigOptionsProvider
        // context.CompilationProvider
        // context.MetadataReferencesProvider
        // context.ParseOptionsProvider
        // context.SyntaxProvider

        // Registerシリーズ
        // context.RegisterImplementationSourceOutput
        // context.RegisterPostInitializationOutput
        // context.RegisterSourceOutput
        context.RegisterPostInitializationOutput(static context =>
            context.AddSource("GenerateBrainfuckMethodAttribute.cs", $$"""
                {{CommentAutoGenerated}}
                using System;
                using System.Diagnostics;
                namespace {{NameSpaceName}} {
                    // [Conditional("COMPILE_TIME_ONLY")]
                    [AttributeUsage(AttributeTargets.Method, AllowMultiple =false, Inherited = false)]
                    internal sealed class {{ClassNameBrainfuckAttribution}} : Attribute
                    {
                        /// <summary>
                        /// generate brainfuck method.
                        /// </summary>
                        /// <param name="source">brainfuck source.</param>
                        internal {{ClassNameBrainfuckAttribution}}(string source) { }
                        /// <summary>Increment the data pointer by one (to point to the next cell to the right).</summary>
                        public string {{nameof(BrainfuckOptions.IncrementPointer)}} = "{{BrainfuckOptionsDefault.IncrementPointer}}";
                        /// <summary>Decrement the data pointer by one (to point to the next cell to the left).</summary>
                        public string {{nameof(BrainfuckOptions.DecrementPointer)}} = "{{BrainfuckOptionsDefault.DecrementPointer}}";
                        /// <summary>Increment the byte at the data pointer by one.</summary>
                        public string {{nameof(BrainfuckOptions.IncrementCurrent)}} = "{{BrainfuckOptionsDefault.IncrementCurrent}}";
                        /// <summary>Decrement the byte at the data pointer by one.</summary>
                        public string {{nameof(BrainfuckOptions.DecrementCurrent)}} = "{{BrainfuckOptionsDefault.DecrementCurrent}}";
                        /// <summary>Output the byte at the data pointer.</summary>
                        public string {{nameof(BrainfuckOptions.Output)}} = "{{BrainfuckOptionsDefault.Output}}";
                        /// <summary>Accept one byte of input, storing its value in the byte at the data pointer.</summary>
                        public string {{nameof(BrainfuckOptions.Input)}} = "{{BrainfuckOptionsDefault.Input}}";
                        /// <summary>If the byte at the data pointer is zero, then instead of moving the instruction pointer forward to the next command, jump it forward to the command after the matching ] command.</summary>
                        public string {{nameof(BrainfuckOptions.Begin)}} = "{{BrainfuckOptionsDefault.Begin}}";
                        /// <summary>If the byte at the data pointer is nonzero, then instead of moving the instruction pointer forward to the next command, jump it back to the command after the matching [ command.</summary>
                        public string {{nameof(BrainfuckOptions.End)}} = "{{BrainfuckOptionsDefault.End}}";
                    }
                }
                """));
        var source = context.SyntaxProvider.ForAttributeWithMetadataName(
            "Brainfuck.GenerateBrainfuckMethodAttribute",
            static (node, token) => true,
            static (context, token) => context
        );
        context.RegisterSourceOutput(source, Emit);
    }
}