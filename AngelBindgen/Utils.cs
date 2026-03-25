namespace AngelBindgen;

public static class Utils
{
    /// <summary>
    /// 指定された開始ディレクトリから親をたどって、指定された名前のディレクトリを探す。
    /// </summary>
    /// <param name="startPath">開始ディレクトリのパス</param>
    /// <param name="targetDirName">探すディレクトリ名</param>
    /// <returns>見つかったディレクトリのフルパス。見つからなければ null。</returns>
    public static string? FindAncestorDirectory(string startPath, string targetDirName)
    {
        var dir = new DirectoryInfo(startPath);

        while (dir != null)
        {
            if (dir.Name.Equals(targetDirName, StringComparison.OrdinalIgnoreCase))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        return null;
    }

    public static string CombineAndGetFullPath(params string[] paths)
    {
        return Path.GetFullPath(Path.Combine(paths));
    }

    public static string ExtractRelativePath(string path, string root)
    {
        string unifiedPath = path.Replace('\\', '/');

        int index = unifiedPath.LastIndexOf(root + "/");

        return index >= 0 ? unifiedPath.Substring(index, unifiedPath.Length - index) : unifiedPath;
    }

    public static string Join(this IEnumerable<string> value, string separator)
    {
        return string.Join(separator, value);
    }
}