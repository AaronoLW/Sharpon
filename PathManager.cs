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

    public void OpenDirectory(string path)
    {
        string fullPath = System.IO.Path.GetFullPath(path);

        if (!Directory.Exists(fullPath))
        {
            Path = null;
            return;
        }

        Path = fullPath;
    }
}
