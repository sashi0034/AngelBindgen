using CppAst;

namespace AngelBindgen;

internal static class ScriptSignatureResolver
{
    public static string ResolveScriptTypeSignature(GeneratorContext ctx, CppType type)
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

            var typeSignature = ResolveScriptTypeSignature(ctx, referenceType.ElementType);
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
            return ResolveScriptTypeSignature(ctx, typedef.ElementType); // FIXME
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

        ctx.SectionLogger.LogError($"Missing type handler: [{type.TypeKind}] {typeFullName}");

        return "";
    }

    public static string ResolveScriptFunctionSignature(GeneratorContext ctx, CppFunction function)
    {
        string result = "";

        var resultType = ResolveScriptTypeSignature(ctx, function.ReturnType);
        if (resultType == "") return "";
        result += resultType;

        result += " " + function.Name + "(";

        for (var index = 0; index < function.Parameters.Count; index++)
        {
            var parameter = function.Parameters[index];

            if (index > 0) result += ", ";

            var parameterType = ResolveScriptTypeSignature(ctx, parameter.Type);
            if (parameterType == "") return "";

            result += parameterType + " " + parameter.Name;
        }

        result += ")";

        return result;
    }
}