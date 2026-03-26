#nullable enable

namespace AngelBindgen;

public class GeneratorConfig
{
    public required string OutputDir { get; init; }

    public required string ProjectNamespace { get; init; }

    public List<string> PredefinedCppTypes { get; init; } = new();

    public Dictionary<string, string> CppTypeToScriptType { get; init; } = new();
}