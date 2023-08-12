
namespace SourceGenerator.Helper.CopyCode.Example2;

[SourceGenerator.Helper.CopyCode.Copy]
internal static partial class Greeter {

    public static string GetHelloWorld() {
        return "Hello Wordl";
    }
}

[SourceGenerator.Helper.CopyCode.Copy]
internal static partial class GreeterTheOter {

    public static string GetHelloWorld() {
        return """"
            Hello Me """{}
            """";
    }
}

