public struct FileManager()
{
    private const string DEFAULT_TEXT = "Text der zum testen gedacht ist (Raphi edition)";

    public string? Path { get; private set; } = null;
    public readonly string DisplayPath => Path ?? "No file opened";

    public TextDocument OpenFile(string? filePath)
    {
        if (filePath == null)
        {
            Path = null;
            return new(DEFAULT_TEXT);
        }

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Couldn't find file at: {filePath}");
        }

        Path = filePath;
        return new(File.ReadAllText(filePath), 0);
    }
}
