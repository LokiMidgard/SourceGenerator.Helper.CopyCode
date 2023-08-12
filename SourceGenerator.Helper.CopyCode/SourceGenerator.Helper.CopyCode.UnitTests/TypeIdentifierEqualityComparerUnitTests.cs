using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace SourceGenerator.Helper.CopyCode.UnitTests;
[TestClass]
public class TypeIdentifierEqualityComparerUnitTests {
    [TestMethod]
    public void Instance_IsSingleton_ReturnsSame() {
        var instance = TypeIdentifierEqualityComparer.Instance;

        Assert.AreSame(TypeIdentifierEqualityComparer.Instance, instance);
    }

    [TestMethod]
    [DataRow("Identifier", "Identifier")]
    [DataRow(null, null)]
    public void Equals_EqualIdentifier_ReturnsTrue(string? left, string? right) {
        TypeDeclarationSyntax? x = left is null ? null : ClassDeclaration(left);
        TypeDeclarationSyntax? y = right is null ? null : ClassDeclaration(right);

        bool areEqual = TypeIdentifierEqualityComparer.Instance.Equals(x, y);

        Assert.IsTrue(areEqual);
    }

    [TestMethod]
    [DataRow("Identifier", "identifier")]
    [DataRow(null, "Identifier")]
    [DataRow("Identifier", null)]
    public void Equals_UnequalIdentifier_ReturnsFalse(string? left, string? right) {
        TypeDeclarationSyntax? x = left is null ? null : ClassDeclaration(left);
        TypeDeclarationSyntax? y = right is null ? null : ClassDeclaration(right);

        bool areEqual = TypeIdentifierEqualityComparer.Instance.Equals(x, y);

        Assert.IsFalse(areEqual);
    }

    [TestMethod]
    public void GetHashCode_NotNull_ReturnsHashCodeOfIdentifier() {
        TypeDeclarationSyntax obj = ClassDeclaration("Identifier");

        int hashCode = TypeIdentifierEqualityComparer.Instance.GetHashCode(obj);

        Assert.AreEqual("Identifier".GetHashCode(), hashCode);
    }

    [TestMethod]
    public void GetHashCode_Null_ReturnsZero() {
        int hashCode = TypeIdentifierEqualityComparer.Instance.GetHashCode(null);

        Assert.AreEqual(0, hashCode);
    }
}
