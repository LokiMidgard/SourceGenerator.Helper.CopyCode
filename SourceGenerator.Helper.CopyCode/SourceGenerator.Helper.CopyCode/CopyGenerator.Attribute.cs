using System.Reflection;

namespace SourceGenerator.Helper.CopyCode;
internal partial class CopyGenerator {
    private static readonly AssemblyName assemblyName = typeof(CopyGenerator).Assembly.GetName();
    private static readonly string generatedCodeAttribute = $@"global::System.CodeDom.Compiler.GeneratedCodeAttribute(""{assemblyName.Name}"", ""{assemblyName.Version}"")";

    private static readonly string copyAttribute = $@"// <auto-generated/>
#nullable enable

namespace SourceGenerator.Helper.CopyCode
{{
	[{generatedCodeAttribute}]
	[global::System.AttributeUsage(global::System.AttributeTargets.Enum | global::System.AttributeTargets.Class | global::System.AttributeTargets.Struct | global::System.AttributeTargets.Interface, AllowMultiple = false)]
	internal sealed class CopyAttribute : global::System.Attribute
	{{
	}}
}}
";
}
