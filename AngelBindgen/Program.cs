using CppAst;

namespace AngelBindgen;

internal static class Program
{
    static void Main()
    {
        var currentDirectory = Environment.CurrentDirectory;
        Console.WriteLine(currentDirectory);

        const string projectName = "AngelBindgen";
        var projectDir = Utils.FindAncestorDirectory(currentDirectory, projectName);
        if (projectDir == null)
        {
            Console.WriteLine("Could not find the project directory.");
            return;
        }

        var targetDir = Utils.CombineAndGetFullPath(projectDir, "../../as-bindings/raylib-5.5");
        var headerFile = Utils.CombineAndGetFullPath(targetDir, "src/raylib.h");
        var headerContent = File.ReadAllText(headerFile);

        var parseOption =
            new CppParserOptions().ConfigureForWindowsMsvc(CppTargetCpu.X86_64, (CppVisualStudioVersion)1944);

        parseOption.Defines.Add("_ALLOW_COMPILER_AND_STL_VERSION_MISMATCH");

        parseOption.AdditionalArguments.Add("-std=c++20");

        parseOption.IncludeFolders.AddRange([
            Utils.CombineAndGetFullPath(targetDir, "src"),
            Utils.CombineAndGetFullPath(targetDir, "src/external/glfw/include"),
        ]);

        // Parse a C++ files
        var compilation = CppParser.Parse(headerContent, parseOption);

        var outputFilepath = Utils.CombineAndGetFullPath(targetDir, "../raylib-as/GeneratedBindings");
        TypeGenerator.Generate(compilation, new GeneratorConfig
        {
            OutputDir = outputFilepath,
            ProjectNamespace = "raylib_as",
            CppTypeToScriptType = new Dictionary<string, string>()
            {
                ["void*"] = "RawPtr@",
            }
        });
    }
}