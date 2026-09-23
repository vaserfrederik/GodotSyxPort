using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

if (args.Length != 1) throw new ArgumentException("output path is required");
var root = Directory.GetCurrentDirectory();
var registry = JsonDocument.Parse(File.ReadAllText(Path.Combine(root, "Porting/parity_registry.json")));
var candidatesByPath = new Dictionary<string, SortedSet<string>>(StringComparer.Ordinal);
foreach (var source in registry.RootElement.GetProperty("entries").EnumerateArray())
{
    var java = source.GetProperty("JavaSource").GetString()!;
    foreach (var file in source.GetProperty("CSharpImplementation").EnumerateArray())
    {
        var path = file.GetString()!;
        if (!candidatesByPath.TryGetValue(path, out var candidates))
            candidatesByPath[path] = candidates = new SortedSet<string>(StringComparer.Ordinal);
        candidates.Add(java);
    }
}

var types = new List<TypeEntry>();
var files = Directory.GetFiles(Path.Combine(root, "Scripts"), "*.cs", SearchOption.AllDirectories)
    .OrderBy(path => path, StringComparer.Ordinal).ToArray();
foreach (var file in files)
{
    var path = Path.GetRelativePath(root, file).Replace('\\', '/');
    var syntax = CSharpSyntaxTree.ParseText(File.ReadAllText(file),
        new CSharpParseOptions(LanguageVersion.Preview));
    var errors = syntax.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
    if (errors.Length > 0) throw new InvalidDataException(path + ": " + string.Join("; ", errors.Select(d => d.ToString())));
    var unit = syntax.GetCompilationUnitRoot();
    foreach (var declaration in unit.DescendantNodes().OfType<MemberDeclarationSyntax>())
    {
        string? name = declaration switch
        {
            BaseTypeDeclarationSyntax type => type.Identifier.ValueText,
            DelegateDeclarationSyntax method => method.Identifier.ValueText,
            _ => null,
        };
        if (name is null) continue;
        var namespaces = declaration.Ancestors().OfType<BaseNamespaceDeclarationSyntax>()
            .Reverse().Select(n => n.Name.ToString());
        var parents = declaration.Ancestors().OfType<BaseTypeDeclarationSyntax>()
            .Reverse().Select(n => n.Identifier.ValueText);
        var qualified = string.Join(".", namespaces.Concat(parents).Append(name));
        var candidates = candidatesByPath.GetValueOrDefault(path)?.ToArray() ?? Array.Empty<string>();
        types.Add(new TypeEntry(qualified, path, declaration.Kind().ToString(), candidates,
            candidates.Length == 0 ? "NoCandidate" : "UnverifiedCandidate"));
    }
}
var result = new
{
    schema = 1,
    source_archive_sha256 = registry.RootElement.GetProperty("source_archive_sha256").GetString(),
    csharp_file_count = files.Length,
    csharp_type_count = types.Count,
    policy = "File-level candidates from the old manifest; each C# type requires an independently reviewed Java binding and parity test.",
    types = types.OrderBy(x => x.CSharpFile, StringComparer.Ordinal)
        .ThenBy(x => x.CSharpType, StringComparer.Ordinal).ToArray(),
};
File.WriteAllText(args[0], JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true }) + "\n");
Console.WriteLine($"C# files: {files.Length}; types: {types.Count}; candidates: {candidatesByPath.Count}");

internal sealed record TypeEntry(string CSharpType, string CSharpFile, string Kind,
    string[] JavaCandidates, string Status);
