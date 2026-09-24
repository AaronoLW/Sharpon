public struct PathManager()
{
    public string? Path { get; private set; } = null;
    public readonly string DisplayPath => Path ?? "No file opened";

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
        if (!Directory.Exists(path))
        {
            Path = null;
            return;
        }

        Path = path;
    }
}
