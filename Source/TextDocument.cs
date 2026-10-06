public class TextDocument(string text)
{
    public List<string> Lines => _lines;
    private readonly List<string> _lines = [.. text.Split('\n')];

    public string Text => string.Join("\n", _lines);

    public int CharIndex { get; private set; }
    public int LineIndex { get; private set; }

    public string Line
    {
        get => _lines[LineIndex];
        private set => _lines[LineIndex] = value;
    }

    public void Insert(string text)
    {
        Line = Line.Insert(CharIndex, text);
        CharIndex++;
    }

    public void RemoveBackwards(int count)
    {

    }

    public void MoveLeft()
    {
        if (CharIndex > 0)
            CharIndex--;
    }

    public void MoveRight()
    {
        if (CharIndex < Line.Length)
            CharIndex++;
    }
}
