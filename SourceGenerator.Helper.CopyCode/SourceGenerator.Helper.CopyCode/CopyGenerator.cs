using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace SourceGenerator.Helper.CopyCode;

[Generator(LanguageNames.CSharp)]
internal sealed partial class CopyGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        const string copyAttribute = "SourceGenerator.Helper.CopyCode.CopyAttribute";

        context.RegisterPostInitializationOutput(PostInitializationCallback);

        var provider = context.SyntaxProvider
            .ForAttributeWithMetadataName(copyAttribute, SyntaxProviderPredicate, SyntaxProviderTransform)
            .Where(static method => method != default)
            ;

        context.RegisterSourceOutput(provider, SourceOutputAction);
    }

    private static void PostInitializationCallback(IncrementalGeneratorPostInitializationContext context) {
        context.AddSource("CopyAttribute.g.cs", SourceText.From(copyAttribute, Encoding.UTF8));
    }

    private static bool SyntaxProviderPredicate(SyntaxNode syntaxNode, CancellationToken cancellationToken) {
        return syntaxNode is BaseTypeDeclarationSyntax;
    }

    private static (string name, string code) SyntaxProviderTransform(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken) {

        var syntaxNode = (BaseTypeDeclarationSyntax)context.TargetNode;

        // We need the symbol to get the namespace, using parent syntax could not work with FileScopedNamespaceDeclaration
        INamedTypeSymbol symbol = context.TargetSymbol as INamedTypeSymbol ?? throw new NotSupportedException($"SyntaxNode {syntaxNode} can't get INamedTypeSymbol");
        string? @namespace;
        {
            StringBuilder builder = new StringBuilder();
            var currentNameSpace = symbol.ContainingNamespace;
            if (!currentNameSpace.IsGlobalNamespace) {
                while (currentNameSpace is not null) {

                    var ns = currentNameSpace.IsGlobalNamespace ? "" : currentNameSpace.Name;
                    if (builder.Length != 0 && !currentNameSpace.IsGlobalNamespace) {
                        builder.Insert(0, '.');
                    }
                    builder.Insert(0, ns);
                    currentNameSpace = currentNameSpace.ContainingNamespace;
                }
            }

            @namespace = builder.ToString();
        }

        List<ParentClass> parents = syntaxNode.Parent is TypeDeclarationSyntax typeDeclarationSyntax ? ParentClass.GetParentClasses(typeDeclarationSyntax) : new List<ParentClass>();

        StringBuilder stringBuilder = new();
        using StringWriter writer = new(stringBuilder, CultureInfo.InvariantCulture);
        using IndentedTextWriter source = new(writer, "    ");

        var textToCopy = syntaxNode.ToString();

        var options = (CSharpParseOptions)context.SemanticModel.SyntaxTree.Options;
        var useRawString = options.LanguageVersion >= (LanguageVersion)1100; //cs11

        var maximumNumberOfQuotas = Regex.Matches(textToCopy, "\"\"(\")+").OfType<Match>().Select(x => x.Length).Concat(new[] { 2 }).Max();

        var quotation = new String('"', maximumNumberOfQuotas + 1);

        source.WriteLine("// <auto-generated/>");
        source.WriteLine("#nullable enable");
        source.WriteLine($"namespace SourceGenerator.Helper.CopyCode;");

        source.WriteLine("internal  static partial class Copy {");
        source.Indent++;

        source.WriteLine($"public const string {symbol.ToDisplayString().Replace(".", "")} = {quotation}");
        source.Indent++;

        source.WriteLine("// <auto-generated/>");
        source.WriteLine("#nullable enable");

        source.WriteLine();

        if (@namespace.Length > 0) {
            source.WriteLine($"namespace {@namespace};");
        }

        foreach (var type in parents) {
            source.WriteLine($"partial {type.Keyword} {type.Name} {type.Constraints}{(type.Constraints.Length > 0 ? " " : "")}{{");
            source.Indent++;
        }

        // We want to copy only the Attributes below the copy Attribute
        var attributesToSkip = syntaxNode.AttributeLists.TakeWhile(list => !list.Attributes.Any(x => x == (SyntaxNode?)context.Attributes.First().ApplicationSyntaxReference!.GetSyntax())).Count() + 1;
        var newAttributiList = syntaxNode.AttributeLists;

        // Full qualify the name
        newAttributiList = new SyntaxList<AttributeListSyntax>(syntaxNode.AttributeLists.Select(x =>
        x.WithAttributes(new SeparatedSyntaxList<AttributeSyntax>().AddRange(
            x.Attributes.Select(attributeConstructor => {
                var sym = context.SemanticModel.GetSymbolInfo(attributeConstructor).Symbol;
                if (sym is IMethodSymbol method && method.ContainingSymbol is INamedTypeSymbol namedSymbol) {
                    var newfull = namedSymbol
                        .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                    attributeConstructor = attributeConstructor.WithName(SyntaxFactory.ParseName(newfull));
                }
                return attributeConstructor;
            })))));

        for (int i = 0; i < attributesToSkip; i++) {
            newAttributiList = newAttributiList.RemoveAt(0);
        }

        var syntaxToPrint = syntaxNode.WithAttributeLists(newAttributiList);

  



            // Write single lines so identation works
            var splitter = syntaxToPrint.NormalizeWhitespace(indentation: "    ", eol: "\n").ToString().AsSpan().Split('\n');
        foreach (var item in splitter) {
            source.WriteLine(item.ToString());
        }

        for (int i = 0; i < parents.Count; i++) {
            source.Indent--;
            source.WriteLine("}");
        }
        source.WriteLine($"{quotation};");
        source.Indent--;
        source.Indent--;
        source.WriteLine("}");

        return (symbol.ToDisplayString(), stringBuilder.ToString());
    }

    private static void SourceOutputAction(SourceProductionContext context, (string name, string code) data) {
        context.AddSource($"{data.name}.Copy.g.cs", data.code);
    }
}

internal class ParentClass {
    public ParentClass(string keyword, string name, string constraints) {
        Keyword = keyword;
        Name = name;
        Constraints = constraints;
    }

    public string Keyword { get; }
    public string Name { get; }
    public string Constraints { get; }

    public static List<ParentClass> GetParentClasses(TypeDeclarationSyntax typeSyntax) {
        var list = new List<ParentClass>();
        GetParentClassesInternal(typeSyntax, list);
        return list;
    }
    private static void GetParentClassesInternal(TypeDeclarationSyntax typeSyntax, List<ParentClass> list) {
        TypeDeclarationSyntax? parentSyntax = typeSyntax;

        while (parentSyntax != null && IsAllowedKind(parentSyntax.Kind())) {
            ParentClass parentClassInfo = new(
                keyword: parentSyntax.Keyword.ValueText,
                name: parentSyntax.Identifier.ToString() + parentSyntax.TypeParameterList,
                constraints: parentSyntax.ConstraintClauses.ToString());

            parentSyntax = (parentSyntax.Parent as TypeDeclarationSyntax);
            if (parentClassInfo is not null) {
                list.Add(parentClassInfo);
            }
        }

    }

    private static bool IsAllowedKind(SyntaxKind kind) =>
        kind is SyntaxKind.ClassDeclaration or
        SyntaxKind.InterfaceDeclaration or
        SyntaxKind.StructDeclaration or
        SyntaxKind.RecordDeclaration;

}

