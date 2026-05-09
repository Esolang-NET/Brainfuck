using Esolang.Brainfuck.Generator.Sequences;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static Esolang.Brainfuck.BrainfuckSequence;

namespace Esolang.Brainfuck.Generator;

public partial class MethodGenerator
{
    static EmittedMethod? Emit(SourceProductionContext context, GeneratorAttributeSyntaxContext source, LanguageVersion currentLanguageVersion)
    {
        var format = SymbolDisplayFormat.FullyQualifiedFormat
            .WithMiscellaneousOptions(
                SymbolDisplayMiscellaneousOptions.EscapeKeywordIdentifiers |
                SymbolDisplayMiscellaneousOptions.UseSpecialTypes |
                SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);
        var methodSymbol = (IMethodSymbol)source.TargetSymbol;
        var methodDeclarationSyntax = (MethodDeclarationSyntax)source.TargetNode;
        if (!IsLanguageVersionAtLeastCSharp8(currentLanguageVersion))
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.LanguageVersionTooLow,
                    methodDeclarationSyntax.Identifier.GetLocation(),
                    methodSymbol.Name,
                    currentLanguageVersion.ToString()));
        }
        if (!TryGetSources(context, methodSymbol, methodDeclarationSyntax, out var sequences))
        {
            var diagnostic = DiagnosticDescriptors.InvalidValueParameter;
            context.ReportDiagnostic(
                Diagnostic.Create(
                    diagnostic,
                    methodDeclarationSyntax.Identifier.GetLocation(),
                    methodSymbol.Name));
            return EmitErrorMethod(methodSymbol, methodDeclarationSyntax, diagnostic.Id,
                string.Format(diagnostic.MessageFormat.ToString(), methodSymbol.Name));
        }
        if (!TryGetReturnType(methodSymbol.ReturnType,
            sequences,
            context,
            methodDeclarationSyntax,
            out var returnType))
        {
            var diagnostic = DiagnosticDescriptors.InvalidReturnType;
            var displayString = methodSymbol.ReturnType.ToDisplayString();
            context.ReportDiagnostic(
                Diagnostic.Create(
                    diagnostic,
                    methodDeclarationSyntax.Identifier.GetLocation(),
                    displayString));
            return EmitErrorMethod(methodSymbol, methodDeclarationSyntax, diagnostic.Id,
                string.Format(diagnostic.MessageFormat.ToString(), displayString));
        }
        if (!TryGetParameterOptions(methodSymbol, returnType, methodSymbol.ReturnType.ToString(), sequences, context, methodDeclarationSyntax, out var parameterOptions, out var dest))
            return EmitErrorMethod(methodSymbol, methodDeclarationSyntax, dest.Descriptor.Id, dest.Message);
        if (sequences.RequiredOutput
            && (returnType & (ReturnType.String | ReturnType.Byte | ReturnType.Enumerable)) == 0
            && string.IsNullOrEmpty(parameterOptions.VaribalePipeWriter)
            && string.IsNullOrEmpty(parameterOptions.VariableTextWriter))
        {
            // Missing required output interface.
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.RequiredOutputInterface,
                    methodDeclarationSyntax.Identifier.GetLocation())
            );
        }
        if (sequences.RequiredInput
            && string.IsNullOrEmpty(parameterOptions.VariablePipeReader)
            && string.IsNullOrEmpty(parameterOptions.VariableInputString)
            && string.IsNullOrEmpty(parameterOptions.VariableTextReader))
        {
            // Missing required input interface.
            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.RequiredInputInterface,
                    methodDeclarationSyntax.Identifier.GetLocation())
            );
        }

        var (openingDefinitionCode, codeForClosingDefinition) = Utils.GenerateOpeningClosingTypeDefinitionCode(methodSymbol);
        var methodModifier = $"{SyntaxFacts.GetText(methodSymbol.DeclaredAccessibility)}{(methodSymbol.IsStatic ? " static" : string.Empty)} partial";
        InternalOptions writeOption = new(
            Space: SPACE,
            VariableStack: STACK_NAME,
            VariableStackIndex: STACK_INDEX,
            VariableCancellationToken: parameterOptions.VariableCancellation,
            VariablePipeWriter: parameterOptions.VaribalePipeWriter,
            VariableTextWriter: parameterOptions.VariableTextWriter,
            VariablePipeReader: parameterOptions.VariablePipeReader,
            VariableTextReader: parameterOptions.VariableTextReader,
            VariableInputString: parameterOptions.VariableInputString,
            ReturnType: returnType
        );
        var returnTypeSyntax = methodSymbol.ReturnType.ToDisplayString(format);
        var methodBodyCode = GenerateMethodBodyCode(2, sequences, ref writeOption, methodSymbol);
        var withAsync = writeOption.ReturnType.IsAsync() && writeOption.UseAwait ? "async" : string.Empty;

        var generatedSourceCode = $$"""
            {{openingDefinitionCode}}
                {{methodModifier}} {{withAsync}} {{returnTypeSyntax}} {{methodSymbol.Name}}({{parameterOptions.ParameterSymbols}})
                {
            {{methodBodyCode}}
                }
            {{codeForClosingDefinition}}

            """;
        return new EmittedMethod(generatedSourceCode, writeOption.UseListAsMemory);
        static EmittedMethod EmitErrorMethod(
            IMethodSymbol methodSymbol,
            MethodDeclarationSyntax methodSyntax,
            string errorId,
            string message)
        {
            var sb = new StringBuilder();
            var (openingDefinitionCode, codeForClosingDefinition) = Utils.GenerateOpeningClosingTypeDefinitionCode(methodSymbol);
            sb.Append($$"""
        {{openingDefinitionCode}}
        """);

            var accessibility = $"{SyntaxFacts.GetText(methodSymbol.DeclaredAccessibility)}{(methodSymbol.IsStatic ? " static" : string.Empty)} partial";
            var returnType = methodSymbol.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var parameters = string.Join(", ", methodSymbol.Parameters.Select(FormatParameter));

            sb.Append("    ").Append(accessibility).Append(" partial ").Append(returnType)
              .Append(' ').Append(methodSymbol.Name).Append('(').Append(parameters).AppendLine(")");
            sb.AppendLine("    {");
            sb.AppendLine($"        throw new global::System.NotImplementedException(\"{errorId}: {message}\");");
            sb.AppendLine("    }");

            sb.Append($$"""
        {{codeForClosingDefinition}}
        """);


            return new EmittedMethod(sb.ToString(), false);

            static string FormatParameter(IParameterSymbol parameter)
            {
                var modifier = parameter.RefKind switch
                {
                    RefKind.In => "in ",
                    RefKind.Out => "out ",
                    RefKind.Ref => "ref ",
                    _ => string.Empty,
                };

                var paramsPrefix = parameter.IsParams ? "params " : string.Empty;
                var typeName = parameter.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                return $"{paramsPrefix}{modifier}{typeName} {parameter.Name}";
            }
        }

        static bool TryGetReturnType(
            ITypeSymbol returnType,
            BrainfuckSequenceEnumerable sequences,
            SourceProductionContext context,
            MethodDeclarationSyntax methodDeclarationSyntax,
            [NotNullWhen(true)] out ReturnType returnTypeResult)
        {
            returnTypeResult = default!;
            var returnType_ = (INamedTypeSymbol)returnType;
            var typeName = returnType_.ToDisplayString(NullableFlowState.NotNull, SymbolDisplayFormat.FullyQualifiedFormat);
            var nullable = returnType_.NullableAnnotation;
            var innerNullable = returnType_.TypeArgumentNullableAnnotations.FirstOrDefault();
            #region return void 
            // void
            const string VOID_TYPE = "void";
            // int
            const string INT_TYPE = "int";
            // Task
            const string VOID_TASK_TYPE = "global::System.Threading.Tasks.Task";
            // Task<int>
            const string INT_TASK_TYPE = "global::System.Threading.Tasks.Task<int>";
            // ValueTask
            const string VOID_VALUETASK_TYPE = "global::System.Threading.Tasks.ValueTask";
            // ValueTask<int>
            const string INT_VALUETASK_TYPE = "global::System.Threading.Tasks.ValueTask<int>";
            #endregion
            #region return string
            // string
            const string STRING_TYPE = "string";
            // Task<string>
            const string STRING_TASK_TYPE = "global::System.Threading.Tasks.Task<string>";
            // ValueTask<string>
            const string STRING_VALUETASK_TYPE = "global::System.Threading.Tasks.ValueTask<string>";
            #endregion
            #region return enumerable byte
            // IEnumerable<byte>
            const string BYTE_ENUMERABLE_TYPE = "global::System.Collections.Generic.IEnumerable<byte>";
            // IAsyncEnumerable<byte>
            const string BYTE_ASYNCENUMERABLE_TYPE = "global::System.Collections.Generic.IAsyncEnumerable<byte>";
            #endregion
            if ((typeName, nullable, innerNullable, sequences.RequiredOutput) switch
            {
                #region return void 
                (VOID_TYPE, _, _, _) => ReturnType.Void,
                (INT_TYPE, _, _, _) => ReturnType.Int,
                (VOID_TASK_TYPE, _, _, _) => ReturnType.Void | ReturnType.Task,
                (INT_TASK_TYPE, _, _, _) => ReturnType.Int | ReturnType.Task,
                (VOID_VALUETASK_TYPE, _, _, _) => ReturnType.Void | ReturnType.ValueTask,
                (INT_VALUETASK_TYPE, _, _, _) => ReturnType.Int | ReturnType.ValueTask,
                #endregion
                #region return string
                (STRING_TYPE, NullableAnnotation.None or NullableAnnotation.Annotated, _, _) => ReturnType.String | ReturnType.Nullable,
                (STRING_TYPE, _, _, _) => ReturnType.String,
                (STRING_TASK_TYPE, NullableAnnotation.None or NullableAnnotation.NotAnnotated, NullableAnnotation.None or NullableAnnotation.Annotated, _) => ReturnType.String | ReturnType.Task | ReturnType.Nullable,
                (STRING_TASK_TYPE, NullableAnnotation.None or NullableAnnotation.NotAnnotated, _, _) => ReturnType.String | ReturnType.Task,
                (STRING_VALUETASK_TYPE, NullableAnnotation.None or NullableAnnotation.NotAnnotated, NullableAnnotation.None or NullableAnnotation.Annotated, _) => ReturnType.String | ReturnType.ValueTask | ReturnType.Nullable,
                (STRING_VALUETASK_TYPE, NullableAnnotation.None or NullableAnnotation.NotAnnotated, _, _) => ReturnType.String | ReturnType.ValueTask,
                #endregion
                #region return enumerable byte
                (BYTE_ENUMERABLE_TYPE, _, _, _) => ReturnType.Byte | ReturnType.Enumerable,
                (BYTE_ASYNCENUMERABLE_TYPE, _, _, _) => ReturnType.Byte | ReturnType.Enumerable | ReturnType.ValueTask,
                #endregion
                _ => (ReturnType?)null,
            } is not { } type)
            {
                // not found support returntype.
                return false;
            }
            returnTypeResult = type;
            return true;
        }
        static bool TryGetParameterOptions(
            IMethodSymbol methodSymbol,
            ReturnType returnType,
            string returnTypeName,
            BrainfuckSequenceEnumerable sequences,
            SourceProductionContext context,
            MethodDeclarationSyntax methodDeclarationSyntax,
            [NotNullWhen(true)] out ParameterOptions parameterOptions,
            [NotNullWhen(false)] out (DiagnosticDescriptor Descriptor, string Message) dest)
        {
            parameterOptions = default!;
            dest = default;
            const string CANCELLATION_TOKEN = "global::System.Threading.CancellationToken";
            const string STRING_TYPE = "string";
            const string PIPE_WRITER_TYPE = "global::System.IO.Pipelines.PipeWriter";
            const string PIPE_READER_TYPE = "global::System.IO.Pipelines.PipeReader";
            const string TEXT_WRITER_TYPE = "global::System.IO.TextWriter";
            const string TEXT_READER_TYPE = "global::System.IO.TextReader";
            var variableCancellation = string.Empty;
            var variablePipeWriter = string.Empty;
            var variablePipeReder = string.Empty;
            var variableTextWriter = string.Empty;
            var variableTextReader = string.Empty;
            var variableInputString = string.Empty;
            List<string>? builder = null;
            foreach (var param in methodSymbol.Parameters)
            {
                var typeName = param.Type.ToDisplayString(NullableFlowState.NotNull, SymbolDisplayFormat.FullyQualifiedFormat);

                if (typeName is CANCELLATION_TOKEN)
                {
                    if (!string.IsNullOrEmpty(variableCancellation))
                    {
                        var diagnostic = DiagnosticDescriptors.DuplicateParameter;
                        // Duplicate declarations are not allowed.
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                diagnostic,
                                methodDeclarationSyntax.GetLocation(),
                                typeName)
                        );
                        dest = (diagnostic, string.Format(diagnostic.MessageFormat.ToString(), typeName));
                        return false;
                    }
                    variableCancellation = param.Name;
                    (builder ??= new()).Add($"{param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {variableCancellation}");
                    continue;
                }
                if (typeName is STRING_TYPE)
                {
                    if (!sequences.RequiredInput)
                    {
                        var diagnostic = DiagnosticDescriptors.UnusedInputParameter;
                        // Input parameter present but source does not use input — report as Hidden.
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                diagnostic,
                                methodDeclarationSyntax.GetLocation(),
                                typeName)
                        );
                    }
                    if (!string.IsNullOrEmpty(variablePipeReder) || !string.IsNullOrEmpty(variableTextReader))
                    {
                        var diagnostic = DiagnosticDescriptors.NotSupportParameterPattern;
                        // Only one input source is allowed.
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                diagnostic,
                                methodDeclarationSyntax.GetLocation())
                        );
                        dest = (diagnostic, diagnostic.MessageFormat.ToString());
                        return false;
                    }
                    if (!string.IsNullOrEmpty(variableInputString))
                    {
                        var diagnostic = DiagnosticDescriptors.DuplicateParameter;
                        // Duplicate declarations are not allowed.
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                diagnostic,
                                methodDeclarationSyntax.GetLocation(),
                                typeName)
                        );
                        dest = (diagnostic, string.Format(diagnostic.MessageFormat.ToString(), typeName));
                        return false;
                    }
                    variableInputString = param.Name;
                    (builder ??= new()).Add($"{param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {variableInputString}");
                    continue;
                }
                if (typeName is PIPE_READER_TYPE)
                {
                    if (!sequences.RequiredInput)
                    {
                        var diagnostic = DiagnosticDescriptors.UnusedInputParameter;
                        // Input parameter present but source does not use input — report as Hidden.
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                diagnostic,
                                methodDeclarationSyntax.GetLocation(),
                                typeName)
                        );
                    }
                    if (!string.IsNullOrEmpty(variableInputString) || !string.IsNullOrEmpty(variableTextReader))
                    {
                        var diagnostic = DiagnosticDescriptors.NotSupportParameterPattern;
                        // Only one input source is allowed.
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                diagnostic,
                                methodDeclarationSyntax.GetLocation())
                        );
                        dest = (diagnostic, diagnostic.MessageFormat.ToString());
                        return false;
                    }
                    // Duplicate declarations are not allowed.
                    if (!string.IsNullOrEmpty(variablePipeReder))
                    {
                        var diagnostic = DiagnosticDescriptors.DuplicateParameter;
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                diagnostic,
                                methodDeclarationSyntax.GetLocation(),
                                typeName)
                        );
                        dest = (diagnostic, string.Format(diagnostic.MessageFormat.ToString(), typeName));
                        return false;
                    }
                    variablePipeReder = param.Name;
                    (builder ??= new()).Add($"{param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {variablePipeReder}");
                    continue;
                }
                if (typeName is TEXT_READER_TYPE)
                {
                    if (!sequences.RequiredInput)
                    {
                        var diagnostic = DiagnosticDescriptors.UnusedInputParameter;
                        // Input parameter present but source does not use input - report as Hidden.
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                diagnostic,
                                methodDeclarationSyntax.GetLocation(),
                                typeName)
                        );
                    }
                    if (!string.IsNullOrEmpty(variableInputString) || !string.IsNullOrEmpty(variablePipeReder))
                    {
                        var diagnostic = DiagnosticDescriptors.NotSupportParameterPattern;
                        // Only one input source is allowed.
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                diagnostic,
                                methodDeclarationSyntax.GetLocation())
                        );
                        dest = (diagnostic, diagnostic.MessageFormat.ToString());
                        return false;
                    }
                    if (!string.IsNullOrEmpty(variableTextReader))
                    {
                        var diagnostic = DiagnosticDescriptors.DuplicateParameter;
                        // Duplicate declarations are not allowed.
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                diagnostic,
                                methodDeclarationSyntax.GetLocation(),
                                typeName)
                        );
                        dest = (diagnostic, string.Format(diagnostic.MessageFormat.ToString(), typeName));
                        return false;
                    }
                    variableTextReader = param.Name;
                    (builder ??= new()).Add($"{param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {variableTextReader}");
                    continue;
                }
                if (typeName is PIPE_WRITER_TYPE)
                {
                    if ((returnType & (ReturnType.String | ReturnType.Enumerable)) > 0)
                    {
                        var diagnostic = DiagnosticDescriptors.NotSupportParameterAndReturnTypePattern;
                        // Not allowed when return type uses string or enumerable mode.
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                diagnostic,
                                methodDeclarationSyntax.GetLocation(),
                                typeName,
                                returnTypeName)
                        );
                        dest = (diagnostic, string.Format(diagnostic.MessageFormat.ToString(), typeName, returnTypeName));
                        return false;
                    }
                    if (!string.IsNullOrEmpty(variablePipeWriter))
                    {
                        // Duplicate declarations are not allowed.
                        var diagnostic = DiagnosticDescriptors.DuplicateParameter;
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                diagnostic,
                                methodDeclarationSyntax.GetLocation(),
                                typeName)
                        );
                        dest = (diagnostic, string.Format(diagnostic.MessageFormat.ToString(), typeName));
                        return false;
                    }
                    if (!string.IsNullOrEmpty(variableTextWriter))
                    {
                        var diagnostic = DiagnosticDescriptors.DuplicateParameter;
                        // Only one output sink is allowed.
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                diagnostic,
                                methodDeclarationSyntax.GetLocation(),
                                typeName)
                        );
                        dest = (diagnostic, string.Format(diagnostic.MessageFormat.ToString(), typeName));
                        return false;
                    }
                    variablePipeWriter = param.Name;
                    (builder ??= new()).Add($"{param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {variablePipeWriter}");
                    continue;
                }
                if (typeName is TEXT_WRITER_TYPE)
                {
                    if ((returnType & (ReturnType.String | ReturnType.Enumerable)) > 0)
                    {
                        var diagnostic = DiagnosticDescriptors.NotSupportParameterAndReturnTypePattern;
                        // Not allowed when return type uses string or enumerable mode.
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                diagnostic,
                                methodDeclarationSyntax.GetLocation(),
                                typeName,
                                returnTypeName)
                        );
                        dest = (diagnostic, string.Format(diagnostic.MessageFormat.ToString(), typeName, returnTypeName));
                        return false;
                    }
                    if (!string.IsNullOrEmpty(variableTextWriter) || !string.IsNullOrEmpty(variablePipeWriter))
                    {
                        var diagnostic = DiagnosticDescriptors.DuplicateParameter;
                        // Only one output sink is allowed.
                        context.ReportDiagnostic(
                            Diagnostic.Create(
                                diagnostic,
                                methodDeclarationSyntax.GetLocation(),
                                typeName)
                        );
                        dest = (diagnostic, string.Format(diagnostic.MessageFormat.ToString(), typeName));
                        return false;
                    }
                    variableTextWriter = param.Name;
                    (builder ??= new()).Add($"{param.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)} {variableTextWriter}");
                    continue;
                }
                {
                    var diagnostic = DiagnosticDescriptors.InvalidParameter;
                    context.ReportDiagnostic(
                        Diagnostic.Create(
                            diagnostic,
                            methodDeclarationSyntax.GetLocation(),
                            typeName)
                    );
                    dest = (diagnostic, string.Format(diagnostic.MessageFormat.ToString(), typeName));
                    return false;
                }
            }
            parameterOptions = new(
                ParameterSymbols: builder?.Count > 0 ? string.Join(", ", builder) : string.Empty,
                VariableCancellation: variableCancellation,
                VaribalePipeWriter: variablePipeWriter,
                VariableTextWriter: variableTextWriter,
                VariablePipeReader: variablePipeReder,
                VariableTextReader: variableTextReader,
                VariableInputString: variableInputString
            );
            return true;
        }
    }
    const string SPACE = "    ";
    const string STACK_NAME = "stack";
    const string STACK_INDEX = "stackIndex";

    static bool IsLanguageVersionAtLeastCSharp8(LanguageVersion languageVersion)
        => languageVersion switch
        {
            LanguageVersion.Default => true,
            LanguageVersion.Latest => true,
            LanguageVersion.Preview => true,
            LanguageVersion.LatestMajor => true,
            _ => languageVersion >= LanguageVersion.CSharp8,
        };

    /// <summary>
    /// Generates the method body code for the specified Brainfuck sequence.
    /// </summary>
    /// <param name="indent">Indent level (4 spaces per level).</param>
    /// <param name="sequences">The command sequence.</param>
    /// <param name="options">Internal generation options.</param>
    /// <param name="methodSymbol">The method symbol for which the code is being generated.</param> 
    /// <returns>The generated method body source code.</returns>
    static string GenerateMethodBodyCode(int indent, BrainfuckSequenceEnumerable sequences, ref InternalOptions options, IMethodSymbol methodSymbol)
    {
        var builder = new StringBuilder();
        var SPACE = options.Space;
        var space = string.Join("", Enumerable.Range(0, indent).Select(v => SPACE));
        var returnType = options.ReturnType;
        var pipeWriter = options.VariablePipeWriter;
        var pipeReader = options.VariablePipeReader;
        var textWriter = options.VariableTextWriter;
        var textReader = options.VariableTextReader;
        var isAsync = options.ReturnType.IsAsync();
        builder.AppendLine($$"""
            {{space}}var {{options.VariableStack}} = new global::System.Collections.Generic.List<byte>(){ 0 };
            {{space}}var {{options.VariableStackIndex}} = 0;
            """);
        if (sequences.RequiredOutput)
        {
            if (string.IsNullOrEmpty(pipeWriter) && !string.IsNullOrEmpty(textWriter))
            {
                pipeWriter = "pipeWriter";
                options = options with
                {
                    VariablePipeWriter = pipeWriter,
                };

                builder.AppendLine($"""
                    {space}var outputPipe = new global::System.IO.Pipelines.Pipe();
                    {space}var {pipeWriter} = outputPipe.Writer;
                    """);
            }
            // This declaration is required when returning string output.
            if (string.IsNullOrEmpty(pipeWriter)
                && (returnType & ReturnType.String) == ReturnType.String)
            {
                pipeWriter = "pipeWriter";
                options = options with
                {
                    VariablePipeWriter = pipeWriter,
                };

                builder.AppendLine($"""
                    {space}var outputPipe = new global::System.IO.Pipelines.Pipe();
                    {space}var {pipeWriter} = outputPipe.Writer;
                    """);
            }
        }
        if (sequences.RequiredInput)
        {
            if (!string.IsNullOrEmpty(options.VariableInputString))
            {
                pipeReader = "pipeReader";
                options = options with
                {
                    VariablePipeReader = pipeReader,
                };
                builder.AppendLine($$"""
                    {{space}}global::System.IO.Pipelines.PipeReader {{pipeReader}};
                    {{space}}{
                    {{space}}{{SPACE}}var inputPipe = new global::System.IO.Pipelines.Pipe();
                    {{space}}{{SPACE}}var bytes = string.IsNullOrEmpty({{options.VariableInputString}}) ? global::System.Array.Empty<byte>() : global::System.Text.Encoding.UTF8.GetBytes({{options.VariableInputString}});
                    {{space}}{{SPACE}}if (bytes.Length > 0)
                    """);
                if (isAsync)
                {
                    var withCancel = string.IsNullOrEmpty(options.VariableCancellationToken) ? string.Empty : ", " + options.VariableCancellationToken;
                    builder.AppendLine($$"""
                        {{space}}{{SPACE}}{{SPACE}}await inputPipe.Writer.WriteAsync(bytes{{withCancel}});
                        {{space}}{{SPACE}}await inputPipe.Writer.CompleteAsync();
                        """);
                    options = options with
                    {
                        UseAwait = true,
                    };
                }
                else
                {
                    builder.AppendLine($$"""
                        {{space}}{{SPACE}}{{SPACE}}global::System.MemoryExtensions.AsSpan(bytes).CopyTo(inputPipe.Writer.GetSpan(bytes.Length));
                        {{space}}{{SPACE}}inputPipe.Writer.Advance(bytes.Length);
                        {{space}}{{SPACE}}inputPipe.Writer.Complete();
                        """);
                }
                builder.AppendLine($$"""
                    {{space}}{{SPACE}}{{pipeReader}} = inputPipe.Reader;
                    {{space}}}
                    """);
            }
            else if (!string.IsNullOrEmpty(textReader) && string.IsNullOrEmpty(pipeReader))
            {
                pipeReader = "pipeReader";
                options = options with
                {
                    VariablePipeReader = pipeReader,
                };
                builder.AppendLine($$"""
                    {{space}}global::System.IO.Pipelines.PipeReader {{pipeReader}};
                    {{space}}{
                    {{space}}{{SPACE}}var inputPipe = new global::System.IO.Pipelines.Pipe();
                    """);
                if (isAsync)
                {
                    builder.AppendLine($$"""
                        {{space}}{{SPACE}}var text = await {{textReader}}.ReadToEndAsync();
                        {{space}}{{SPACE}}var bytes = string.IsNullOrEmpty(text) ? global::System.Array.Empty<byte>() : global::System.Text.Encoding.UTF8.GetBytes(text);
                        {{space}}{{SPACE}}if (bytes.Length > 0)
                        {{space}}{{SPACE}}{{SPACE}}await inputPipe.Writer.WriteAsync(bytes);
                        {{space}}{{SPACE}}await inputPipe.Writer.CompleteAsync();
                        """);
                    options = options with
                    {
                        UseAwait = true,
                    };
                }
                else
                {
                    builder.AppendLine($$"""
                        {{space}}{{SPACE}}var text = {{textReader}}.ReadToEnd();
                        {{space}}{{SPACE}}var bytes = string.IsNullOrEmpty(text) ? global::System.Array.Empty<byte>() : global::System.Text.Encoding.UTF8.GetBytes(text);
                        {{space}}{{SPACE}}if (bytes.Length > 0)
                        {{space}}{{SPACE}}{
                        {{space}}{{SPACE}}{{SPACE}}global::System.MemoryExtensions.AsSpan(bytes).CopyTo(inputPipe.Writer.GetSpan(bytes.Length));
                        {{space}}{{SPACE}}{{SPACE}}inputPipe.Writer.Advance(bytes.Length);
                        {{space}}{{SPACE}}}
                        {{space}}{{SPACE}}inputPipe.Writer.Complete();
                        """);
                }
                builder.AppendLine($$"""
                    {{space}}{{SPACE}}{{pipeReader}} = inputPipe.Reader;
                    {{space}}}
                    """);
            }
        }

        var seq = sequences.Select((v, i) => new Sequence(i, v.Sequence, v.Syntax)).ToArray().AsMemory();
        var nest = seq.Nest();
        WriteNest(indent, nest, builder, ref options);
        var isEnumerable = options.ReturnType.IsEnumerable();
        var withCancellation = string.IsNullOrEmpty(options.VariableCancellationToken) ? string.Empty : ", " + options.VariableCancellationToken;
        if (!isEnumerable)
        {
            if ((options.ReturnType & ReturnType.String) > 0)
            {
                if (!sequences.RequiredOutput)
                {
                    if ((options.ReturnType & ReturnType.ValueTask) > 0)
                    {
                        var returnType_ = (INamedTypeSymbol)methodSymbol.ReturnType;
                        var innerType = returnType_.TypeArguments.First();
                        var annoation = returnType_.TypeArgumentNullableAnnotations.First();
                        if ((options.ReturnType & ReturnType.Nullable) > 0)
                        {
                            var bang = annoation is NullableAnnotation.None ? "!" : string.Empty;
                            // ValueTask<string?> is a struct; cannot return null directly. Use default value.
                            builder.AppendLine($$"""
                                {{space}}return new global::System.Threading.Tasks.ValueTask<{{innerType.ToDisplayString()}}>(default({{innerType.ToDisplayString()}}){{bang}});
                                """);
                        }
                        else
                        {
                            builder.AppendLine($$"""
                                {{space}}return new global::System.Threading.Tasks.ValueTask<string>(string.Empty);
                                """);
                        }
                    }
                    else if ((options.ReturnType & ReturnType.Task) > 0)
                    {
                        var returnType_ = (INamedTypeSymbol)methodSymbol.ReturnType;
                        var innerType = returnType_.TypeArguments.First();
                        var annoation = returnType_.TypeArgumentNullableAnnotations.First();
                        if ((options.ReturnType & ReturnType.Nullable) > 0)
                        {
                            var bang = annoation is NullableAnnotation.None ? "!" : string.Empty;
                            builder.AppendLine($$"""
                                {{space}}return global::System.Threading.Tasks.Task.FromResult<{{innerType.ToDisplayString()}}>(default({{innerType.ToDisplayString()}}){{bang}});
                                """);
                        }
                        else
                        {
                            builder.AppendLine($$"""
                                {{space}}return global::System.Threading.Tasks.Task.FromResult(string.Empty);
                                """);
                        }
                    }
                    else
                    {
                        builder.AppendLine($$"""
                            {{space}}return null!;
                            """);
                    }
                }
                else if (isAsync)
                {
                    builder.AppendLine($$"""
                        {{space}}{
                        {{space}}{{SPACE}}await {{pipeWriter}}.CompleteAsync();
                        #if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                        {{space}}{{SPACE}}await using var stream = new global::System.IO.MemoryStream();
                        #else
                        {{space}}{{SPACE}}using var stream = new global::System.IO.MemoryStream();
                        #endif
                        {{space}}{{SPACE}}using var reader = new global::System.IO.StreamReader(stream, System.Text.Encoding.UTF8, false, 1024, true);
                        {{space}}{{SPACE}}await outputPipe.Reader.CopyToAsync(stream{{withCancellation}});
                        {{space}}{{SPACE}}stream.Seek(0, global::System.IO.SeekOrigin.Begin);
                        {{space}}{{SPACE}}if (stream.Length == 0) return null!;
                        {{space}}{{SPACE}}var returnString = (await reader.ReadToEndAsync()).TrimEnd('\0');
                        {{space}}{{SPACE}}if (returnString.Length == 0) return null!;
                        {{space}}{{SPACE}}return returnString;
                        {{space}}}
                        """);
                    options = options with
                    {
                        UseAwait = true,
                    };
                }
                else
                {
                    builder.AppendLine($$"""
                        {{space}}{
                        {{space}}{{SPACE}}{{pipeWriter}}.Complete();
                        {{space}}{{SPACE}}if (!outputPipe.Reader.TryRead(out var outputResult))
                        {{space}}{{SPACE}}{{SPACE}}return null!;
                        {{space}}{{SPACE}}var resultArray = global::System.Buffers.BuffersExtensions.ToArray(outputResult.Buffer);
                        {{space}}{{SPACE}}outputPipe.Reader.AdvanceTo(outputResult.Buffer.End);
                        {{space}}{{SPACE}}if (resultArray.Length == 0) return null!;
                        {{space}}{{SPACE}}var returnString = global::System.Text.Encoding.UTF8.GetString(resultArray).TrimEnd('\0');
                        {{space}}{{SPACE}}if (returnString.Length == 0) return null!;
                        {{space}}{{SPACE}}return returnString;
                        {{space}}}
                        """);
                }
            }
        }
        else if (!sequences.RequiredOutput)
        {
            if (options.ReturnType == (ReturnType.Byte | ReturnType.Enumerable | ReturnType.ValueTask))
            {
                options = options with
                {
                    UseAwait = true,
                };
                builder.AppendLine($$"""
                    {{space}}await global::System.Threading.Tasks.Task.CompletedTask;
                    """);
            }
            builder.AppendLine($$"""
                {{space}}yield break;
                """);
        }
        if (sequences.RequiredOutput && !string.IsNullOrEmpty(textWriter))
        {
            if (isAsync)
            {
                builder.AppendLine($$"""
                    {{space}}{
                    {{space}}{{SPACE}}await {{pipeWriter}}.CompleteAsync();
                    {{space}}{{SPACE}}if (await outputPipe.Reader.ReadAsync() is { } outputResult)
                    {{space}}{{SPACE}}{
                    {{space}}{{SPACE}}{{SPACE}}var resultArray = global::System.Buffers.BuffersExtensions.ToArray(outputResult.Buffer);
                    {{space}}{{SPACE}}{{SPACE}}outputPipe.Reader.AdvanceTo(outputResult.Buffer.End);
                    {{space}}{{SPACE}}{{SPACE}}if (resultArray.Length > 0)
                    {{space}}{{SPACE}}{{SPACE}}{
                    {{space}}{{SPACE}}{{SPACE}}{{SPACE}}var outputText = global::System.Text.Encoding.UTF8.GetString(resultArray).TrimEnd('\0');
                    {{space}}{{SPACE}}{{SPACE}}{{SPACE}}if (outputText.Length > 0)
                    {{space}}{{SPACE}}{{SPACE}}{{SPACE}}{{SPACE}}await {{textWriter}}.WriteAsync(outputText);
                    {{space}}{{SPACE}}{{SPACE}}}
                    {{space}}{{SPACE}}}
                    {{space}}}
                    """);
                options = options with
                {
                    UseAwait = true,
                };
            }
            else
            {
                builder.AppendLine($$"""
                    {{space}}{
                    {{space}}{{SPACE}}{{pipeWriter}}.Complete();
                    {{space}}{{SPACE}}if (outputPipe.Reader.TryRead(out var outputResult))
                    {{space}}{{SPACE}}{
                    {{space}}{{SPACE}}{{SPACE}}var resultArray = global::System.Buffers.BuffersExtensions.ToArray(outputResult.Buffer);
                    {{space}}{{SPACE}}{{SPACE}}outputPipe.Reader.AdvanceTo(outputResult.Buffer.End);
                    {{space}}{{SPACE}}{{SPACE}}if (resultArray.Length > 0)
                    {{space}}{{SPACE}}{{SPACE}}{
                    {{space}}{{SPACE}}{{SPACE}}{{SPACE}}var outputText = global::System.Text.Encoding.UTF8.GetString(resultArray).TrimEnd('\0');
                    {{space}}{{SPACE}}{{SPACE}}{{SPACE}}if (outputText.Length > 0)
                    {{space}}{{SPACE}}{{SPACE}}{{SPACE}}{{SPACE}}{{textWriter}}.Write(outputText);
                    {{space}}{{SPACE}}{{SPACE}}}
                    {{space}}{{SPACE}}}
                    {{space}}}
                    """);
            }
        }
        if (options.UseListAsMemory)
        {
            builder.AppendLine($$"""
                {{space}}static global::System.Memory<T> AsMemory<T>(global::System.Collections.Generic.List<T> self)
                {{space}}{
                {{space}}{{SPACE}} return new global::System.Memory<T>(global::System.Runtime.CompilerServices.Unsafe.As<ListDummy<T>>(self).Items).Slice(0, self.Count);
                {{space}}}
                """);
        }
        if ((options.ReturnType & ReturnType.Int) > 0 && options.UseAwait)
        {
            if ((options.ReturnType & ReturnType.Task) > 0 || (options.ReturnType & ReturnType.ValueTask) > 0)
            {
                builder.AppendLine($$"""
                    {{space}}return 0;
                    """);
            }
        }
        if (!options.UseAwait)
        {
            if (options.ReturnType is (ReturnType.Void | ReturnType.Task))
                builder.AppendLine($$"""
                    {{space}}return global::System.Threading.Tasks.Task.CompletedTask;
                    """);
            else if (options.ReturnType is ReturnType.Int)
                builder.AppendLine($$"""
                    {{space}}return 0;
                    """);
            else if (options.ReturnType is (ReturnType.Int | ReturnType.Task))
                builder.AppendLine($$"""
                    {{space}}return global::System.Threading.Tasks.Task.FromResult(0);
                    """);
            else if (options.ReturnType is (ReturnType.Void | ReturnType.ValueTask))
                builder.AppendLine($$"""
                    {{space}}return default;
                    """);
            else if (options.ReturnType is (ReturnType.Int | ReturnType.ValueTask))
                builder.AppendLine($$"""
                    {{space}}return new global::System.Threading.Tasks.ValueTask<int>(0);
                    """);
            else if (options.ReturnType is (ReturnType.Byte | ReturnType.Enumerable | ReturnType.ValueTask))
            {
                options = options with
                {
                    UseAwait = true,
                };
                builder.AppendLine($$"""
                    {{space}}await global::System.Threading.Tasks.Task.CompletedTask;
                    """);
            }
        }
        return builder.ToString();
    }
    static void WriteNest(int indent, IEnumerable<INestableSequence> sequences, StringBuilder builder, ref InternalOptions options)
    {
        foreach (var sequence in sequences)
        {
            if (sequence is Sequence simple)
            {
                if (simple is { Value: Begin or End })
                {
                    WriteComment(indent, Comment, simple.Syntax, builder);
                    continue;
                }
                WriteSequence(indent, simple.Value, simple.Syntax, builder, ref options);
                continue;
            }
            if (sequence is NestableSequence nested)
            {
                var begin = nested.Begin;
                WriteSequence(indent, begin.Value, begin.Syntax, builder, ref options);
                WriteNest(indent + 1, nested.Nest, builder, ref options);
                var end = nested.End;
                WriteSequence(indent, end.Value, end.Syntax, builder, ref options);
                continue;
            }
        }
    }
    static void WriteSequence(int indent, BrainfuckSequence sequence, ReadOnlyMemory<char> syntax, StringBuilder builder, ref InternalOptions options)
    {
        WriteComment(indent, sequence, syntax, builder);
        var space = string.Join("", Enumerable.Range(0, indent).Select(v => SPACE));
        var stackIndex = options.VariableStackIndex;
        var stack = options.VariableStack;
        var pipeReader = options.VariablePipeReader;
        var pipeWriter = options.VariablePipeWriter;
        var ct = options.VariableCancellationToken;
        var withCancel = string.IsNullOrEmpty(ct) ? string.Empty : ", " + ct;
        var isAsync = options.ReturnType.IsAsync();
        var isEnumerable = options.ReturnType.IsEnumerable();

        builder.AppendLine(sequence switch
        {
            IncrementPointer => $"""
                {space}{stackIndex}++;
                {space}if ({stack}.Count >= {stackIndex}) {stack}.Add(0);
                """,
            DecrementPointer => $"""
                {space}if ({stackIndex} > 0){stackIndex}--;
                """,
            IncrementCurrent => $$"""
                {{space}}{
                {{space}}{{SPACE}}var value = {{stack}}[{{stackIndex}}];
                {{space}}{{SPACE}}{{stack}}[{{stackIndex}}] = unchecked((byte)(value + 1));
                {{space}}}
                """,
            DecrementCurrent => $$"""
                {{space}}{
                {{space}}{{SPACE}}var value = {{stack}}[{{stackIndex}}];
                {{space}}{{SPACE}}{{stack}}[{{stackIndex}}] = unchecked((byte)(value - 1));
                {{space}}}
                """,
            Begin => $$"""
                {{space}}while({{stack}}[{{stackIndex}}] is not 0) {
                """,
            End => $$"""
                {{space}}}
                """,
            Input => SimpleInput(ref options),
            Output => isEnumerable switch
            {
                true => $"""
                {space}yield return {stack}[{stackIndex}];
                """,
                _ => SimpleOutput(ref options),
            },
            _ => string.Empty,
        });
        string SimpleInput(ref InternalOptions options)
        {
            options = options with
            {
                UseListAsMemory = true,
            };
            if (isAsync)
            {
                options = options with
                {
                    UseAwait = true,
                };
                return $$"""
                    {{space}}{
                    {{space}}{{SPACE}}if (await {{pipeReader}}.ReadAtLeastAsync(1{{withCancel}}) is { } result && result.Buffer.Length >= 0)
                    {{space}}{{SPACE}}{
                    {{space}}{{SPACE}}{{SPACE}}var readableSeq = result.Buffer.Slice(result.Buffer.Start, 1);
                    {{space}}{{SPACE}}{{SPACE}}global::System.Buffers.BuffersExtensions.CopyTo(readableSeq, AsMemory({{stack}}).Slice({{stackIndex}}, 1).Span);
                    {{space}}{{SPACE}}{{SPACE}}{{pipeReader}}.AdvanceTo(readableSeq.End);
                    {{space}}{{SPACE}}}
                    {{space}}}
                    """;
            }
            return $$"""
                {{space}}{
                {{space}}{{SPACE}}if ({{pipeReader}}.TryRead(out var result) && result.Buffer.Length >= 0)
                {{space}}{{SPACE}}{
                {{space}}{{SPACE}}{{SPACE}}var readableSeq = result.Buffer.Slice(result.Buffer.Start, 1);
                {{space}}{{SPACE}}{{SPACE}}global::System.Buffers.BuffersExtensions.CopyTo(readableSeq, AsMemory({{stack}}).Slice({{stackIndex}}, 1).Span);
                {{space}}{{SPACE}}{{SPACE}}{{pipeReader}}.AdvanceTo(readableSeq.End);
                {{space}}{{SPACE}}}
                {{space}}}
                """;
        }
        string SimpleOutput(ref InternalOptions options)
        {
            options = options with
            {
                UseListAsMemory = true,
            };
            if (isAsync)
            {
                options = options with
                {
                    UseAwait = true,
                };
                return $"""
                    {space}await {pipeWriter}.WriteAsync(AsMemory({stack}).Slice({stackIndex},1){withCancel});
                    """;
            }
            return $"""
                {space}AsMemory({stack}).Slice({stackIndex}, 1).Span.CopyTo({pipeWriter}.GetSpan(1));
                {space}{pipeWriter}.Advance(1);
                """;
        }
    }
    static void WriteComment(int indent, BrainfuckSequence sequence, ReadOnlyMemory<char> syntax, StringBuilder builder)
    {
        var space = string.Join("", Enumerable.Range(0, indent).Select(v => SPACE));
        var comment = syntax.ToString().Replace("\r", "\\r").Replace("\n", "\\n");
        builder.AppendLine($"{space}// {sequence}:{comment}");
    }

    static bool TryGetSources(
        SourceProductionContext context,
        IMethodSymbol methodSymbol,
        MethodDeclarationSyntax methodDeclarationSyntax,
        out BrainfuckSequenceEnumerable sequences
    )
    {
        sequences = default!;
        var attributeData = methodSymbol.GetAttributes().Single(
            x => x.AttributeClass?.ToDisplayString() == NameSpaceName + "." + ClassNameBrainfuckAttribution
        );

        if (attributeData.ConstructorArguments is not { Length: > 0 }
            || attributeData.ConstructorArguments[0] is not { IsNull: false, Value: string source }
            || string.IsNullOrEmpty(source))
        {
            return false;
        }
        var incrementPointer = GetNamedArgumentOrDefault(attributeData, nameof(BrainfuckOptions.IncrementPointer), BrainfuckOptionsDefault.IncrementPointer);
        var decrementPointer = GetNamedArgumentOrDefault(attributeData, nameof(BrainfuckOptions.DecrementPointer), BrainfuckOptionsDefault.DecrementPointer);
        var incrementCurrent = GetNamedArgumentOrDefault(attributeData, nameof(BrainfuckOptions.IncrementCurrent), BrainfuckOptionsDefault.IncrementCurrent);
        var decrementCurrent = GetNamedArgumentOrDefault(attributeData, nameof(BrainfuckOptions.DecrementCurrent), BrainfuckOptionsDefault.DecrementCurrent);
        var output = GetNamedArgumentOrDefault(attributeData, nameof(BrainfuckOptions.Output), BrainfuckOptionsDefault.Output);
        var input = GetNamedArgumentOrDefault(attributeData, nameof(BrainfuckOptions.Input), BrainfuckOptionsDefault.Input);
        var begin = GetNamedArgumentOrDefault(attributeData, nameof(BrainfuckOptions.Begin), BrainfuckOptionsDefault.Begin);
        var end = GetNamedArgumentOrDefault(attributeData, nameof(BrainfuckOptions.End), BrainfuckOptionsDefault.End);
        sequences = new BrainfuckSequenceEnumerable(source!.AsMemory(), new BrainfuckOptions(
            IncrementPointer: incrementPointer,
            DecrementPointer: decrementPointer,
            IncrementCurrent: incrementCurrent,
            DecrementCurrent: decrementCurrent,
            Output: output,
            Input: input,
            Begin: begin,
            End: end
        ));
        return true;
        static T GetNamedArgumentOrDefault<T>(AttributeData attributeData, string name, T defaultValue)
        {
            // ImmutbaleArray<T> does not have a Find method...
            foreach (var namedArgument in attributeData.NamedArguments)
            {
                if (StringComparer.Ordinal.Equals(namedArgument.Key, name))
                {
                    return (T)namedArgument.Value.Value!;
                }
            }
            return defaultValue;
        }
    }
}
internal record InternalOptions(
    string Space,
    string VariableStack,
    string VariableStackIndex,
    string VariablePipeWriter,
    string VariableTextWriter,
    string VariablePipeReader,
    string VariableTextReader,
    string VariableCancellationToken,
    string VariableInputString,
    ReturnType ReturnType,
    bool UseListAsMemory = false,
    bool UseAwait = false
);
internal record ParameterOptions(
    string ParameterSymbols,
    string VariableCancellation,
    string VaribalePipeWriter,
    string VariableTextWriter,
    string VariablePipeReader,
    string VariableTextReader,
    string VariableInputString
);
internal enum ParameterType
{
    None = default,
    String,
    ByteArray,
    ReadOnlyMemoryChar,

}
/// <summary>
/// Encodes return information in 000_0_0000 form, ordered as TaskType | IsEnumerable | ReturnType from left to right.
/// </summary>
[Flags]
internal enum ReturnType
{
    /// <summary>
    /// No return value.
    /// </summary>
    Void = 0b_0_000_0_0001,
    /// <summary>
    /// Returns an exit code integer.
    /// </summary>
    Int = 0b_0_000_0_0010,
    /// <summary>
    /// Returns a string.
    /// </summary>
    String = 0b_0_000_0_0100,
    /// <summary>
    /// Returns a byte.
    /// </summary>
    Byte = 0b_0_000_0_1000,
    /// <summary>
    /// Returns an enumerable sequence.
    /// </summary>
    Enumerable = 0b_0_000_1_0000,
    /// <summary>
    /// Return value is wrapped in <see cref="Task"/>.
    /// </summary>
    Task = 0b_0_001_0_0000,
    /// <summary>
    /// Return value is wrapped in <see cref="ValueTask"/>.
    /// </summary>
    ValueTask = 0b_0_010_0_0000,
    /// <summary>
    /// Return value is Nullable type.
    /// </summary>
    Nullable = 0b_1_000_0_0000,
}
static class OptionsExtensions
{
    public static bool IsEnumerable(this ReturnType returnType) => (returnType & ReturnType.Enumerable) > 0;
    public static bool IsAsync(this ReturnType returnType) => (returnType & (ReturnType.Task | ReturnType.ValueTask)) > 0;
}
