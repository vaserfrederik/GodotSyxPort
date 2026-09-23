using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

if (args.Length != 1) throw new ArgumentException("output path is required");
var root = Directory.GetCurrentDirectory();
var output = new List<FileEntry>();
foreach (var file in Directory.GetFiles(Path.Combine(root, "Scripts"), "*.cs", SearchOption.AllDirectories)
             .OrderBy(path => path, StringComparer.Ordinal))
{
    var path = Path.GetRelativePath(root, file).Replace('\\', '/');
    var syntax = CSharpSyntaxTree.ParseText(File.ReadAllText(file),
        new CSharpParseOptions(LanguageVersion.Preview));
    var errors = syntax.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
    if (errors.Length > 0) throw new InvalidDataException(path + ": " + string.Join("; ", errors.Select(d => d.ToString())));
    var unit = syntax.GetCompilationUnitRoot();
    var calls = new Dictionary<CallKey, int>();
    var methods = unit.DescendantNodes().OfType<BaseMethodDeclarationSyntax>()
        .Select(m => new MethodEntry(Caller(m.ParameterList), m switch
        {
            MethodDeclarationSyntax declaration => declaration.Identifier.ValueText,
            ConstructorDeclarationSyntax => ".ctor",
            OperatorDeclarationSyntax operation => operation.OperatorToken.ValueText,
            ConversionOperatorDeclarationSyntax conversion => conversion.Type.ToString(),
            _ => ""
        }, m.ParameterList.Parameters.Count))
        .Where(m => m.Name.Length > 0)
        .OrderBy(m => m.Caller, StringComparer.Ordinal).ToArray();
    foreach (var invocation in unit.DescendantNodes().OfType<InvocationExpressionSyntax>())
    {
        var (receiver, name) = invocation.Expression switch
        {
            SimpleNameSyntax simple => ("", simple.Identifier.ValueText),
            MemberAccessExpressionSyntax member => (IdentifierPath(member.Expression), member.Name.Identifier.ValueText),
            _ => ("", "")
        };
        if (name.Length == 0) continue;
        Add(new CallKey(Caller(invocation), "invoke", receiver, name, invocation.ArgumentList.Arguments.Count));
    }
    foreach (var creation in unit.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
    {
        var target = IdentifierPath(creation.Type);
        if (target.Length == 0) continue;
        Add(new CallKey(Caller(creation), "construct", target, ".ctor", creation.ArgumentList?.Arguments.Count ?? 0));
    }
    output.Add(new FileEntry(path, methods, calls.OrderBy(kv => kv.Key.Caller, StringComparer.Ordinal)
        .ThenBy(kv => kv.Key.Kind, StringComparer.Ordinal)
        .ThenBy(kv => kv.Key.Receiver, StringComparer.Ordinal)
        .ThenBy(kv => kv.Key.Name, StringComparer.Ordinal)
        .ThenBy(kv => kv.Key.Arity)
        .Select(kv => new CallEntry(kv.Key.Caller, kv.Key.Kind, kv.Key.Receiver,
            kv.Key.Name, kv.Key.Arity, kv.Value)).ToArray()));

    void Add(CallKey key) => calls[key] = calls.GetValueOrDefault(key) + 1;
}

File.WriteAllText(args[0], JsonSerializer.Serialize(new { schema = 1, files = output },
    new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"C# call inventory: {output.Count} files, {output.Sum(x => x.Calls.Sum(y => y.Count))} calls");

static string Caller(SyntaxNode node)
{
    var namespaces = node.Ancestors().OfType<BaseNamespaceDeclarationSyntax>()
        .Reverse().Select(n => n.Name.ToString());
    var types = node.Ancestors().OfType<BaseTypeDeclarationSyntax>()
        .Reverse().Select(n => n.Identifier.ValueText);
    var member = node.Ancestors().FirstOrDefault(n => n is BaseMethodDeclarationSyntax
        or LocalFunctionStatementSyntax or AccessorDeclarationSyntax);
    var method = member switch
    {
        MethodDeclarationSyntax m => m.Identifier.ValueText + "/" + m.ParameterList.Parameters.Count,
        ConstructorDeclarationSyntax c => ".ctor/" + c.ParameterList.Parameters.Count,
        LocalFunctionStatementSyntax l => l.Identifier.ValueText + "/" + l.ParameterList.Parameters.Count,
        OperatorDeclarationSyntax o => o.OperatorToken.ValueText + "/" + o.ParameterList.Parameters.Count,
        ConversionOperatorDeclarationSyntax c => c.Type + "/" + c.ParameterList.Parameters.Count,
        AccessorDeclarationSyntax a => a.Keyword.ValueText + "/0",
        _ => "<initializer>/0"
    };
    return string.Join(".", namespaces.Concat(types)) + "#" + method;
}

static string IdentifierPath(SyntaxNode node) => node switch
{
    IdentifierNameSyntax id => id.Identifier.ValueText,
    GenericNameSyntax generic => generic.Identifier.ValueText,
    QualifiedNameSyntax qualified => Join(IdentifierPath(qualified.Left), IdentifierPath(qualified.Right)),
    AliasQualifiedNameSyntax alias => Join(alias.Alias.Identifier.ValueText, alias.Name.Identifier.ValueText),
    MemberAccessExpressionSyntax member => Join(IdentifierPath(member.Expression), member.Name.Identifier.ValueText),
    _ => ""
};

static string Join(string left, string right) => left.Length == 0 || right.Length == 0 ? "" : left + "." + right;

internal sealed record FileEntry(string CSharpFile, MethodEntry[] Methods, CallEntry[] Calls);
internal sealed record MethodEntry(string Caller, string Name, int Arity);
internal sealed record CallEntry(string Caller, string Kind, string Receiver, string Name, int Arity, int Count);
internal sealed record CallKey(string Caller, string Kind, string Receiver, string Name, int Arity);
