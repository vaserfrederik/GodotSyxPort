using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

if (args.Length != 1) throw new ArgumentException("output path is required");
var root = Directory.GetCurrentDirectory();
var output = new List<FileEntry>();
var parsedFiles = Directory.GetFiles(Path.Combine(root, "Scripts"), "*.cs", SearchOption.AllDirectories)
    .OrderBy(path => path, StringComparer.Ordinal)
    .Select(file =>
    {
    var syntax = CSharpSyntaxTree.ParseText(File.ReadAllText(file),
        new CSharpParseOptions(LanguageVersion.Preview));
    var errors = syntax.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
    if (errors.Length > 0) throw new InvalidDataException(file + ": " + string.Join("; ", errors.Select(d => d.ToString())));
    return (File: file, Unit: syntax.GetCompilationUnitRoot());
    }).ToArray();
var memberTypes = BuildMemberTypes(parsedFiles.Select(x => x.Unit));
foreach (var (file, unit) in parsedFiles)
{
    var path = Path.GetRelativePath(root, file).Replace('\\', '/');
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
        Add(new CallKey(Caller(invocation), "invoke", receiver,
            ReceiverTypeHint(invocation, receiver, memberTypes), name, invocation.ArgumentList.Arguments.Count));
    }
    foreach (var creation in unit.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
    {
        var target = IdentifierPath(creation.Type);
        if (target.Length == 0) continue;
        Add(new CallKey(Caller(creation), "construct", target, "", ".ctor",
            creation.ArgumentList?.Arguments.Count ?? 0));
    }
    output.Add(new FileEntry(path, methods, calls.OrderBy(kv => kv.Key.Caller, StringComparer.Ordinal)
        .ThenBy(kv => kv.Key.Kind, StringComparer.Ordinal)
        .ThenBy(kv => kv.Key.Receiver, StringComparer.Ordinal)
        .ThenBy(kv => kv.Key.ReceiverTypeHint, StringComparer.Ordinal)
        .ThenBy(kv => kv.Key.Name, StringComparer.Ordinal)
        .ThenBy(kv => kv.Key.Arity)
        .Select(kv => new CallEntry(kv.Key.Caller, kv.Key.Kind, kv.Key.Receiver,
            kv.Key.ReceiverTypeHint, kv.Key.Name, kv.Key.Arity, kv.Value)).ToArray()));

    void Add(CallKey key) => calls[key] = calls.GetValueOrDefault(key) + 1;
}

File.WriteAllText(args[0], JsonSerializer.Serialize(new { schema = 2, files = output },
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
    ThisExpressionSyntax => "this",
    IdentifierNameSyntax id => id.Identifier.ValueText,
    GenericNameSyntax generic => generic.Identifier.ValueText,
    QualifiedNameSyntax qualified => Join(IdentifierPath(qualified.Left), IdentifierPath(qualified.Right)),
    AliasQualifiedNameSyntax alias => Join(alias.Alias.Identifier.ValueText, alias.Name.Identifier.ValueText),
    MemberAccessExpressionSyntax member => Join(IdentifierPath(member.Expression), member.Name.Identifier.ValueText),
    _ => ""
};

static string Join(string left, string right) => left.Length == 0 || right.Length == 0 ? "" : left + "." + right;

static Dictionary<string, Dictionary<string, string>> BuildMemberTypes(IEnumerable<CompilationUnitSyntax> units)
{
    var declarations = units.SelectMany(u => u.DescendantNodes().OfType<TypeDeclarationSyntax>())
        .GroupBy(t => t.Identifier.ValueText);
    var result = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
    foreach (var group in declarations)
    {
        // A short type name must designate just one declared project type.
        var identities = group.Select(t => string.Join(".",
            t.Ancestors().OfType<BaseNamespaceDeclarationSyntax>().Reverse().Select(n => n.Name.ToString())
            .Concat(t.Ancestors().OfType<BaseTypeDeclarationSyntax>().Reverse().Select(n => n.Identifier.ValueText))
            .Append(t.Identifier.ValueText))).Distinct().ToArray();
        if (identities.Length != 1) continue;
        var members = new Dictionary<string, string>(StringComparer.Ordinal);
        var conflicting = new HashSet<string>(StringComparer.Ordinal);
        foreach (var type in group)
        {
            foreach (var field in type.Members.OfType<FieldDeclarationSyntax>())
                foreach (var variable in field.Declaration.Variables)
                    Add(variable.Identifier.ValueText, IdentifierPath(field.Declaration.Type));
            foreach (var property in type.Members.OfType<PropertyDeclarationSyntax>())
                Add(property.Identifier.ValueText, IdentifierPath(property.Type));
        }
        result[group.Key] = members;
        result[identities[0]] = members;

        void Add(string name, string memberType)
        {
            if (memberType.Length == 0 || conflicting.Contains(name)) return;
            if (members.TryGetValue(name, out var previous) && previous != memberType)
            {
                members.Remove(name);
                conflicting.Add(name);
            }
            else members[name] = memberType;
        }
    }
    return result;
}

static string ReceiverTypeHint(SyntaxNode node, string receiver,
    Dictionary<string, Dictionary<string, string>> memberTypes)
{
    if (!receiver.Contains('.')) return SimpleReceiverTypeHint(node, receiver);
    var parts = receiver.Split('.');
    var first = parts[0] == "this" && parts.Length > 1 ? "this." + parts[1] : parts[0];
    var type = SimpleReceiverTypeHint(node, first);
    if (type.Length == 0) return "";
    for (var i = first.StartsWith("this.", StringComparison.Ordinal) ? 2 : 1; i < parts.Length; i++)
    {
        if (!memberTypes.TryGetValue(type, out var members) ||
            !members.TryGetValue(parts[i], out type)) return "";
    }
    return type;
}

static string SimpleReceiverTypeHint(SyntaxNode node, string receiver)
{
    var explicitThis = receiver.StartsWith("this.", StringComparison.Ordinal);
    var name = explicitThis ? receiver[5..] : receiver;
    if (name.Length == 0 || name.Contains('.')) return "";
    var owner = node.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
    if (owner is null) return "";
    if (!explicitThis)
    {
        // A lambda parameter can hide a field or a parameter in its enclosing method.
        foreach (var lambda in node.Ancestors().Where(a => a is LambdaExpressionSyntax
                     or AnonymousMethodExpressionSyntax))
        {
            var parameters = lambda switch
            {
                SimpleLambdaExpressionSyntax simple => new[] { simple.Parameter },
                ParenthesizedLambdaExpressionSyntax parenthesized => parenthesized.ParameterList.Parameters.ToArray(),
                AnonymousMethodExpressionSyntax anonymous => anonymous.ParameterList?.Parameters.ToArray() ?? Array.Empty<ParameterSyntax>(),
                _ => Array.Empty<ParameterSyntax>()
            };
            foreach (var parameter in parameters)
                if (parameter.Identifier.ValueText == name)
                    return parameter.Type is null ? "" : IdentifierPath(parameter.Type);
        }
        foreach (var function in node.Ancestors().OfType<LocalFunctionStatementSyntax>())
            foreach (var parameter in function.ParameterList.Parameters)
                if (parameter.Identifier.ValueText == name)
                    return parameter.Type is null ? "" : IdentifierPath(parameter.Type);
        foreach (var loop in node.Ancestors().OfType<ForEachStatementSyntax>())
            if (loop.Identifier.ValueText == name)
                return IdentifierPath(loop.Type) == "var" ? "" : IdentifierPath(loop.Type);
        foreach (var clause in node.Ancestors().OfType<CatchClauseSyntax>())
            if (clause.Declaration?.Identifier.ValueText == name)
                return IdentifierPath(clause.Declaration.Type);
        var method = node.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();
        if (method is not null)
        {
            // Only use declarations in a containing block, before this call. Declarations
            // in a sibling branch must never give a receiver an invented type.
            foreach (var block in node.Ancestors().OfType<BlockSyntax>())
                foreach (var declaration in block.Statements.OfType<LocalDeclarationStatementSyntax>())
                    if (declaration.SpanStart < node.SpanStart &&
                        declaration.Declaration.Variables.Any(v => v.Identifier.ValueText == name))
                    {
                        var type = declaration.Declaration.Type;
                        if (IdentifierPath(type) != "var") return IdentifierPath(type);
                        var variable = declaration.Declaration.Variables.Single(v => v.Identifier.ValueText == name);
                        return variable.Initializer?.Value is ObjectCreationExpressionSyntax creation
                            ? IdentifierPath(creation.Type) : "";
                    }
            // A non-block local declaration (for example a for initializer) may
            // still shadow a field. Leave it unresolved rather than guessing.
            if (method.DescendantNodes().OfType<VariableDeclaratorSyntax>().Any(v =>
                v.Identifier.ValueText == name && v.Ancestors().Any(a => a is LocalDeclarationStatementSyntax
                    or ForStatementSyntax)))
                return "";
            foreach (var parameter in method.ParameterList.Parameters)
                if (parameter.Identifier.ValueText == name)
                    return parameter.Type is null ? "" : IdentifierPath(parameter.Type);
        }
    }
    foreach (var field in owner.Members.OfType<FieldDeclarationSyntax>())
        if (field.Declaration.Variables.Any(v => v.Identifier.ValueText == name))
            return IdentifierPath(field.Declaration.Type);
    foreach (var property in owner.Members.OfType<PropertyDeclarationSyntax>())
        if (property.Identifier.ValueText == name)
            return IdentifierPath(property.Type);
    return "";
}

internal sealed record FileEntry(string CSharpFile, MethodEntry[] Methods, CallEntry[] Calls);
internal sealed record MethodEntry(string Caller, string Name, int Arity);
internal sealed record CallEntry(string Caller, string Kind, string Receiver, string ReceiverTypeHint,
    string Name, int Arity, int Count);
internal sealed record CallKey(string Caller, string Kind, string Receiver, string ReceiverTypeHint,
    string Name, int Arity);
