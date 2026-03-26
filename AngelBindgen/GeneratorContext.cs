namespace AngelBindgen;

internal class TypeRegisterInfo
{
    public class OutputContent
    {
        public string Name { get; set; } = "";

        public string Definition { get; set; } = "";
    }

    public OutputContent Output { get; } = new();
}

internal class GeneratorContext
{
    public required GeneratorConfig Config { get; init; }

    public SectionLogger SectionLogger { get; } = new();

    public Dictionary<string, TypeRegisterInfo> RegisteredScriptTypes { get; } = new();
}