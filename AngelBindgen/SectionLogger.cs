#nullable enable

namespace AngelBindgen;

public class SectionLogger
{
    private bool _hasError = false;

    public bool HasError => _hasError;

    public void Begin(string sectionName)
    {
        _hasError = false;

        Console.Write($"=== {sectionName} ===");
    }

    public void LogError(string message)
    {
        if (!_hasError)
        {
            Console.WriteLine("");
        }

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(message);
        Console.ResetColor();

        _hasError = true;
    }

    public void End()
    {
        if (!_hasError)
        {
            Console.WriteLine(" OK");
        }
    }
}