using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

if (args is ["--self-test"])
{
    SelfTest();
    return;
}
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
        var hint = ReceiverHint(invocation, receiver);
        Add(new CallKey(Caller(invocation), "invoke", receiver,
            hint.Type, hint.Kind, name, invocation.ArgumentList.Arguments.Count));
    }
    foreach (var creation in unit.DescendantNodes().OfType<ObjectCreationExpressionSyntax>())
    {
        var target = IdentifierPath(creation.Type);
        if (target.Length == 0) continue;
        Add(new CallKey(Caller(creation), "construct", target, "", "", ".ctor",
            creation.ArgumentList?.Arguments.Count ?? 0));
    }
    output.Add(new FileEntry(path, methods, calls.OrderBy(kv => kv.Key.Caller, StringComparer.Ordinal)
        .ThenBy(kv => kv.Key.Kind, StringComparer.Ordinal)
        .ThenBy(kv => kv.Key.Receiver, StringComparer.Ordinal)
        .ThenBy(kv => kv.Key.ReceiverTypeHint, StringComparer.Ordinal)
        .ThenBy(kv => kv.Key.ReceiverHintKind, StringComparer.Ordinal)
        .ThenBy(kv => kv.Key.Name, StringComparer.Ordinal)
        .ThenBy(kv => kv.Key.Arity)
        .Select(kv => new CallEntry(kv.Key.Caller, kv.Key.Kind, kv.Key.Receiver,
            kv.Key.ReceiverTypeHint, kv.Key.ReceiverHintKind,
            kv.Key.Name, kv.Key.Arity, kv.Value)).ToArray()));

    void Add(CallKey key) => calls[key] = calls.GetValueOrDefault(key) + 1;
}

File.WriteAllText(args[0], JsonSerializer.Serialize(new { schema = 3, files = output },
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
    NullableTypeSyntax nullable => IdentifierPath(nullable.ElementType),
    GenericNameSyntax generic => generic.Identifier.ValueText,
    QualifiedNameSyntax qualified => Join(IdentifierPath(qualified.Left), IdentifierPath(qualified.Right)),
    AliasQualifiedNameSyntax alias => Join(alias.Alias.Identifier.ValueText, alias.Name.Identifier.ValueText),
    MemberAccessExpressionSyntax member => Join(IdentifierPath(member.Expression), member.Name.Identifier.ValueText),
    _ => ""
};

static string Join(string left, string right) => left.Length == 0 || right.Length == 0 ? "" : left + "." + right;

static TypeHint ReceiverHint(SyntaxNode node, string receiver)
{
    if (receiver.StartsWith("this.", StringComparison.Ordinal))
    {
        var field = FieldTypeHint(node, receiver);
        return new TypeHint(field, field.Length == 0 ? "" : "field", field.Length > 0);
    }
    if (receiver.Length == 0 || receiver.Contains('.')) return new TypeHint("", "", false);

    foreach (var ancestor in node.Ancestors())
    {
        switch (ancestor)
        {
            case BlockSyntax block:
                foreach (var local in block.Statements.OfType<LocalDeclarationStatementSyntax>()
                             .Where(local => local.SpanStart < node.SpanStart))
                {
                    var hint = DeclaredVariable(local.Declaration, receiver, "local");
                    if (hint.Shadowed) return hint;
                }
                break;
            case ForStatementSyntax loop when loop.Declaration is not null:
            {
                var hint = DeclaredVariable(loop.Declaration, receiver, "local");
                if (hint.Shadowed) return hint;
                break;
            }
            case ForEachStatementSyntax loop when loop.Identifier.ValueText == receiver:
                return Hint(loop.Type, "local");
            case SimpleLambdaExpressionSyntax lambda when lambda.Parameter.Identifier.ValueText == receiver:
                return Hint(lambda.Parameter.Type, "parameter");
            case ParenthesizedLambdaExpressionSyntax lambda:
            {
                var parameter = lambda.ParameterList.Parameters.FirstOrDefault(p => p.Identifier.ValueText == receiver);
                if (parameter is not null) return Hint(parameter.Type, "parameter");
                break;
            }
            case LocalFunctionStatementSyntax local:
            {
                var parameter = local.ParameterList.Parameters.FirstOrDefault(p => p.Identifier.ValueText == receiver);
                if (parameter is not null) return Hint(parameter.Type, "parameter");
                break;
            }
            case BaseMethodDeclarationSyntax method:
            {
                var parameter = method.ParameterList.Parameters.FirstOrDefault(p => p.Identifier.ValueText == receiver);
                if (parameter is not null) return Hint(parameter.Type, "parameter");
                break;
            }
        }
    }
    var fieldType = FieldTypeHint(node, receiver);
    return new TypeHint(fieldType, fieldType.Length == 0 ? "" : "field", fieldType.Length > 0);
}

static TypeHint DeclaredVariable(VariableDeclarationSyntax declaration, string receiver, string kind)
{
    var variable = declaration.Variables.FirstOrDefault(v => v.Identifier.ValueText == receiver);
    if (variable is null) return new TypeHint("", "", false);
    var type = IdentifierPath(declaration.Type);
    if (type == "var")
        type = variable.Initializer?.Value is ObjectCreationExpressionSyntax creation
            ? IdentifierPath(creation.Type) : "";
    return new TypeHint(type, type.Length == 0 ? "" : kind, true);
}

static TypeHint Hint(TypeSyntax? type, string kind)
{
    var name = type is null ? "" : IdentifierPath(type);
    return new TypeHint(name == "var" ? "" : name, name is "" or "var" ? "" : kind, true);
}

static void SelfTest()
{
    const string sample = """
        class Example {
            private Target value;
            private Target field;
            void Run(Target value) {
                Target local = new Target();
                var inferred = new Target();
                var unknown = Factory();
                value.Ping(); this.value.Ping(); field.Ping();
                local.Ping(); inferred.Ping(); unknown.Ping();
            }
        }
        """;
    var tree = CSharpSyntaxTree.ParseText(sample);
    var calls = tree.GetRoot().DescendantNodes().OfType<InvocationExpressionSyntax>()
        .Where(node => node.Expression is MemberAccessExpressionSyntax)
        .Select(node =>
        {
            var member = (MemberAccessExpressionSyntax)node.Expression;
            return (Receiver: IdentifierPath(member.Expression), Hint: ReceiverHint(node, IdentifierPath(member.Expression)));
        }).ToDictionary(entry => entry.Receiver, entry => entry.Hint);
    foreach (var (receiver, kind) in new[] { ("value", "parameter"), ("this.value", "field"),
        ("field", "field"), ("local", "local"), ("inferred", "local") })
        if (calls[receiver].Type != "Target" || calls[receiver].Kind != kind)
            throw new InvalidDataException(receiver + " receiver hint is incorrect");
    if (calls["unknown"].Type.Length != 0 || !calls["unknown"].Shadowed)
        throw new InvalidDataException("unresolved local variable must not inherit field type");
    Console.WriteLine("C# receiver scopes: field, this, parameter, local, var and unknown passed");
}

static string FieldTypeHint(SyntaxNode node, string receiver)
{
    var explicitThis = receiver.StartsWith("this.", StringComparison.Ordinal);
    var name = explicitThis ? receiver[5..] : receiver;
    if (name.Length == 0 || name.Contains('.')) return "";
    var owner = node.Ancestors().OfType<TypeDeclarationSyntax>().FirstOrDefault();
    if (owner is null) return "";
    if (!explicitThis)
    {
        var method = node.Ancestors().OfType<BaseMethodDeclarationSyntax>().FirstOrDefault();
        if (method is not null && (method.ParameterList.Parameters.Any(p => p.Identifier.ValueText == name)
            || method.DescendantNodes().OfType<VariableDeclaratorSyntax>().Any(v =>
                v.Identifier.ValueText == name && v.Ancestors().Any(a => a is LocalDeclarationStatementSyntax))))
            return "";
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
internal sealed record CallEntry(string Caller, string Kind, string Receiver,
    string ReceiverTypeHint, string ReceiverHintKind,
    string Name, int Arity, int Count);
internal sealed record CallKey(string Caller, string Kind, string Receiver,
    string ReceiverTypeHint, string ReceiverHintKind,
    string Name, int Arity);
internal sealed record TypeHint(string Type, string Kind, bool Shadowed);
