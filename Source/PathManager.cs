public struct PathManager(string placeholderText = "Placeholder path text")
{
    public string? Path { get; private set; } = null;

    public readonly string DisplayPath
    {
        get
        {
            string path = Path ?? _placeholderText;

            // I hate this code but it works

            if (Path != null && Directory.Exists(Path) && Path.EndsWith(System.IO.Path.DirectorySeparatorChar))
                path = System.IO.Path.GetDirectoryName(Path) ?? PathExtensions.GetRootDirectory();

            return path;
        }
    }

    private readonly string _placeholderText = placeholderText;

    public TextDocument OpenFile(string? filePath)
    {
        if (filePath == null)
        {
            Path = null;
            return new("");
        }

        string fullPath = System.IO.Path.GetFullPath(filePath);

        if (Directory.Exists(fullPath))
        {
            return new("");
        }

        if (fullPath.EndsWith(System.IO.Path.DirectorySeparatorChar))
        {
            fullPath = fullPath.TrimEnd(System.IO.Path.DirectorySeparatorChar);
        }

        Path = fullPath;
        if (!File.Exists(fullPath))
        {
            return new("");
        }


        string fileContent = File.ReadAllText(fullPath);
        return new(fileContent);
    }

    public bool SaveFile(TextDocument document)
    {
        if (Path == null)
        {
            return false;
        }
        else
        {
            File.WriteAllText(Path, document.Text);
            return true;
        }

    }

    public string[]? OpenDirectory(string? initialPath)
    {
        string path = initialPath ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        path = System.IO.Path.GetFullPath(path);

        if (File.Exists(path) && !Directory.Exists(path))
        {
            path = System.IO.Path.GetDirectoryName(path) ?? Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        else if (!Directory.Exists(path))
        {
            path = TryGetDirectoryFromNonExistingFile(path);
        }

        string[] files = [];
        try
        {
            files = Directory.GetFileSystemEntries(path);
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine(ex.Message);
            return OpenDirectory(Path);
        }

        Path = path;

        string[] filtered = [.. files.Where(f => !File.GetAttributes(f).HasFlag(FileAttributes.Hidden))];
        return filtered;
    }

    private string TryGetDirectoryFromNonExistingFile(string path)
    {
        if (path.EndsWith(System.IO.Path.DirectorySeparatorChar))
        {
            path = path.TrimEnd(System.IO.Path.DirectorySeparatorChar);
        }

        int lastSeparatorIndex = path.LastIndexOf(System.IO.Path.DirectorySeparatorChar);

        if (lastSeparatorIndex == -1)
            throw new Exception("No Path.DirectorySeparatorChar found in path");

        return path[0..lastSeparatorIndex];
    }
}
