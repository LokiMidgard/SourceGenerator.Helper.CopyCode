[![NuGet](https://img.shields.io/nuget/v/SourceGenerator.Helper.CopyCode.svg?style=flat-square)](https://www.nuget.org/packages/SourceGenerator.Helper.CopyCode/)
[![GitHub license](https://img.shields.io/github/license/LokiMidgard/SourceGenerator.Helper.CopyCode.svg?style=flat-square)](https://tldrlegal.com/license/mit-license#summary)

# SourceGenerator.Helper.CopyCode

This Generator is intendede to generate text that a source generator can use to emit source to its generation.

E.g. Instead of writing a String that contains the definiton of an Attribute (without syntax highlighting and checking).
You can generate the attribute normaly in Code and anotate it wit `[SourceGenerator.Helper.CopyCode.Copy]`.
Attributes defined on that Type will also be copied, if they are defined below the `[SourceGenerator.Helper.CopyCode.Copy]`-Attribute.

Assume you have the following attribute:

```c#

namespace SourceGenerator.Helper.CopyCode.Example;

[SourceGenerator.Helper.CopyCode.Copy]
[System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
internal sealed class MyGeneratorAttribute : Attribute {

}
```

then the generator will generate: 
```c#
// <auto-generated/>
#nullable enable
namespace SourceGenerator.Helper.CopyCode;
internal  static partial class Copy {
    public const string SourceGeneratorHelperCopyCodeExampleMyGeneratorAttribute = """
        // <auto-generated/>
        #nullable enable
        
        namespace SourceGenerator.Helper.CopyCode.Example;
        [System.AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
        internal sealed class MyGeneratorAttribute : Attribute
        {
        }
        """;
}
```

And your Generator can emit it:
```c#
[Generator(LanguageNames.CSharp)]
public class MyGenerator : IIncrementalGenerator {
    public void Initialize(IncrementalGeneratorInitializationContext context) {
        context.RegisterPostInitializationOutput(context => context.AddSource("attribute.g.cs", SourceGenerator.Helper.CopyCode.Copy.SourceGeneratorHelperCopyCodeExampleMyGeneratorAttribute ));
        // The rest of your generator…
    }
}
```