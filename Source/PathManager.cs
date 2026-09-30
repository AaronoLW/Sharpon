public struct PathManager(string placeholderText = "Placeholder path text")
{
    public string? Path { get; private set; } = null;

    public readonly string DisplayPath
    {
        get
        {
            string path = Path ?? _placeholderText;

            // I hate this code but it works

            if (Path != null && Directory.Exists(Path) && Path.EndsWith('/'))
                path = System.IO.Path.GetDirectoryName(Path) ?? _placeholderText;

            return path;
        }
    }

    private readonly string _placeholderText = placeholderText;

    public TextDocument OpenFile(string? filePath)
    {
        if (filePath == null)
        {
            Path = null;
            return new(null, 0);
        }

        string fullPath = System.IO.Path.GetFullPath(filePath);

        if (Directory.Exists(fullPath))
        {
            return new(null, 0);
        }

        if (fullPath.EndsWith(System.IO.Path.DirectorySeparatorChar))
        {
            fullPath = fullPath.TrimEnd(System.IO.Path.DirectorySeparatorChar);
        }

        Path = fullPath;
        if (!File.Exists(fullPath))
        {
            return new("", 0);
        }


        string fileContent = File.ReadAllText(fullPath);
        return new(fileContent, fileContent.Length);
    }

    public void SaveFile(TextDocument document)
    {
        if (Path == null)
        {
            Console.WriteLine("Couldn't save file (no file opened)");
        }
        else
        {
            File.WriteAllText(Path, document.Text);
            Console.WriteLine("Saved File!");
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
            Console.WriteLine("duiwahdjiaw");
            path = TryGetDirectoryFromNonExistingFile(path);
        }

        Path = path;

        string[] files = Directory.GetFileSystemEntries(path);

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
