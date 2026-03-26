using System.Text;
using CppAst;

namespace AngelBindgen;

public class GeneratorConfig
{
    public required string OutputDir { get; init; }

    public required string ProjectNamespace { get; init; }

    public List<string> PredefinedCppTypes { get; init; } = new();

    public Dictionary<string, string> CppTypeToScriptType { get; init; } = new();
}

public static class TypeGenerator
{
    private class TypeRegisterInfo
    {
    }

    private class GeneratorContext
    {
        public required GeneratorConfig Config { get; init; }

        public FileReporter Reporter { get; } = new();

        public Dictionary<string, TypeRegisterInfo> RegisteredScriptTypes { get; } = new();
    }

    public class FileReporter
    {
        private bool _hasError = false;

        public bool HasError => _hasError;

        public void Begin(string filename)
        {
            _hasError = false;

            Console.Write($"=== {filename} ===");
        }

        public void ReportError(string message)
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

    public static void Generate(CppCompilation ast, GeneratorConfig generatorConfig)
    {
        var targetScope = ast;
        // var targetScope = ast.Namespaces.FirstOrDefault(n => n.Name == "");
        // if (targetScope == null)
        // {
        //     Console.WriteLine("Could not find the target namespace.");
        //     return;
        // }

        GeneratorContext ctx = new GeneratorContext { Config = generatorConfig };

        foreach (var type in generatorConfig.PredefinedCppTypes)
        {
            ctx.RegisteredScriptTypes[type] = new TypeRegisterInfo();
        }

        foreach (var class_ in targetScope.Classes)
        {
            string className = class_.Name;
            ctx.RegisteredScriptTypes[className] = new TypeRegisterInfo();
        }

        foreach (var class_ in targetScope.Classes)
        {
            generateClass(ctx, class_);
        }
    }

    private static string getScriptTypeSignature(GeneratorContext ctx, CppType type)
    {
        switch (type.TypeKind)
        {
        case CppTypeKind.Primitive:
            var primitiveType = (CppPrimitiveType)type;
            return primitiveType.Kind switch
            {
                CppPrimitiveKind.Void => "void",
                CppPrimitiveKind.Bool => "bool",
                CppPrimitiveKind.WChar => "",
                CppPrimitiveKind.Char => "int8",
                CppPrimitiveKind.Short => "int16",
                CppPrimitiveKind.Int => "int32",
                CppPrimitiveKind.Long => "",
                CppPrimitiveKind.LongLong => "int64",
                CppPrimitiveKind.UnsignedChar => "uint8",
                CppPrimitiveKind.UnsignedShort => "uint16",
                CppPrimitiveKind.UnsignedInt => "uint32",
                CppPrimitiveKind.UnsignedLong => "",
                CppPrimitiveKind.UnsignedLongLong => "uint64",
                CppPrimitiveKind.Float => "float",
                CppPrimitiveKind.Double => "double",
                CppPrimitiveKind.LongDouble => "",
                _ => throw new ArgumentOutOfRangeException()
            };
        case CppTypeKind.Pointer:
            break;
        case CppTypeKind.Reference:
            var referenceType = (CppReferenceType)type;

            var typeSignature = getScriptTypeSignature(ctx, referenceType.ElementType);
            if (typeSignature == "") break;

            return typeSignature + "&";
        case CppTypeKind.Array:
            break;
        case CppTypeKind.Qualified:
            break;
        case CppTypeKind.Function:
            break;
        case CppTypeKind.Typedef:
            var typedef = (CppTypedef)type;
            return getScriptTypeSignature(ctx, typedef.ElementType); // FIXME
        case CppTypeKind.StructOrClass:
            var class_ = (CppClass)type;
            if (ctx.RegisteredScriptTypes.ContainsKey(class_.Name))
            {
                return class_.Name;
            }

            break;
        case CppTypeKind.Enum:
            break;
        case CppTypeKind.TemplateParameterType:
            break;
        case CppTypeKind.TemplateParameterNonType:
            break;
        case CppTypeKind.TemplateArgumentType:
            break;
        case CppTypeKind.Unexposed:
            break;
        default:
            throw new ArgumentOutOfRangeException();
        }

        // throw new NotImplementedException("stringifyType not implemented.");

        // TypeMappings からのマッピングを試みる
        var typeFullName = type.FullName;
        if (ctx.Config.CppTypeToScriptType.TryGetValue(typeFullName.Replace(" ", ""), out var mappedType))
        {
            return mappedType;
        }

        ctx.Reporter.ReportError($"Missing type handler: [{type.TypeKind}] {typeFullName}");

        return "";
    }

    private static string getScriptFunctionSignature(GeneratorContext ctx, CppFunction function)
    {
        string result = "";

        var resultType = getScriptTypeSignature(ctx, function.ReturnType);
        if (resultType == "") return "";
        result += resultType;

        result += " " + function.Name + "(";

        for (var index = 0; index < function.Parameters.Count; index++)
        {
            var parameter = function.Parameters[index];

            if (index > 0) result += ", ";

            var parameterType = getScriptTypeSignature(ctx, parameter.Type);
            if (parameterType == "") return "";

            result += parameterType + " " + parameter.Name;
        }

        result += ")";

        return result;
    }

    private static void generateClass(GeneratorContext ctx, CppClass class_)
    {
        string className = class_.Name;
        string includePath = Utils.ExtractRelativePath(class_.SourceFile, "src");

        var outputFilename = className + ".generated.cpp";

        ctx.Reporter.Begin(outputFilename);

        var sb = new StringBuilder();

        foreach (var field in class_.Fields)
        {
            var typeSignature = getScriptTypeSignature(ctx, field.Type);
            if (typeSignature == "") continue;

            sb.AppendLine(
                $$"""
                  bind.property("{{getScriptTypeSignature(ctx, field.Type)}} {{field.Name}}", &{{className}}::{{field.Name}});
                  """);
        }

        foreach (var method in class_.Functions)
        {
            if (method.IsStatic) continue;

            string functionSignature = getScriptFunctionSignature(ctx, method);
            if (functionSignature == "") continue;

            string parameterTypes = method.Parameters.Select(p => p.Type.FullName).Join(", ");

            string const_ = method.IsConst ? ", const_" : "";

            sb.AppendLine(
                $$"""
                  bind.method(
                    "{{functionSignature}}",
                    overload_cast<{{parameterTypes}}>(&{{className}}::{{method.Name}}{{const_}}));
                  """);
        }

        string content =
            $$"""
              // This file is auto-generated by AngelBindgen

              #include "raylib.h"

              #include <asbind20/asbind.hpp>
              #include <asbind20/operators.hpp>

              #ifdef AS_USE_NAMESPACE
              using namespace AngelScript;
              #endif

              namespace {{ctx.Config.ProjectNamespace}}
              {
                  std::function<void()> BindScript_{{className}}(asIScriptEngine* engine)
                  {
                      using namespace asbind20;
                      auto bind = asbind20::value_class<{{className}}>(engine, "{{className}}", asOBJ_POD | asOBJ_APP_CLASS_ALLINTS);
                      bind.behaviours_by_traits();
                      
                      return [engine, bind]() mutable{
                          {{sb.ToString().Trim().Replace(Environment.NewLine, Environment.NewLine + "            ")}}
                      };
                  }
              }
              """;

        var outputFilepath = Utils.CombineAndGetFullPath(ctx.Config.OutputDir, outputFilename);

        File.WriteAllText(Utils.CombineAndGetFullPath(outputFilepath, ""), content);

        ctx.Reporter.End();
    }
}