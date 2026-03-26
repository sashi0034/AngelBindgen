namespace AngelBindgen;

internal class TypeRegisterInfo
{
}

internal class GeneratorContext
{
    public required GeneratorConfig Config { get; init; }

    public SectionLogger Logger { get; } = new();

    public Dictionary<string, TypeRegisterInfo> RegisteredScriptTypes { get; } = new();
}