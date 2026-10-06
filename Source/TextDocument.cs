using System.Numerics;

public class TextDocument(string text)
{
    public List<string> Lines => _lines;
    private readonly List<string> _lines = [.. text.Split('\n')];

    public string Text => string.Join("\n", _lines);

    public int CharIndex { get; private set; }
    public int LineIndex { get; private set; }

    private CaretStyle _caretStyle = CaretStyle.Beam;

    public CaretStyle CaretStyle
    {
        get => _caretStyle;
        set { _caretStyle = value; ClampCaretIfNecessary(); }
    }

    public string Line
    {
        get => _lines[LineIndex];
        private set => _lines[LineIndex] = value;
    }

    public int LineLength => Math.Max(Line.Length - (CaretStyle == CaretStyle.Block ? 1 : 0), 0);

    public Vector2 GetCaretPosition(Vector2 offset, float pointSize, float lineSpacing)
    {
        float x = App.Font.MeasureString(Line[0..CharIndex], pointSize).X;
        float y = lineSpacing * LineIndex;

        return offset + new Vector2(x, y);
    }

    public void Insert(string text)
    {
        Line = Line.Insert(CharIndex, text);
        CharIndex++;

        if (CharIndex > LineLength)
            CharIndex--;
    }

    public void RemoveBackwards(int count)
    {
        if (CharIndex == 0)
        {
            JoinLineUpwards();
            return;
        }

        Line = Line.Remove(CharIndex - count, count);
        CharIndex -= count;
    }

    public void DeleteLine()
    {
        if (LineIndex == 0)
        {
            _lines[0] = "";
        }
        else
        {
            _lines.RemoveAt(LineIndex);
            LineIndex--;
        }
    }

    public void MoveLeft(int count = 1)
    {
        if (CharIndex - (count - 1) > 0)
            CharIndex -= count;
    }

    public void MoveRight(int count = 1)
    {
        if (CharIndex + (count - 1) < LineLength)
            CharIndex += count;
    }

    public void MoveUp()
    {
        if (LineIndex > 0)
        {
            LineIndex--;
            ClampCaretIfNecessary();
        }

    }

    public void MoveDown()
    {
        if (LineIndex < Lines.Count - 1)
        {
            LineIndex++;
            ClampCaretIfNecessary();
        }
    }

    public void InsertNewLine()
    {
        string text = Line[CharIndex..LineLength];
        Line = Line[0..CharIndex];

        Lines.Insert(LineIndex + 1, text);
        LineIndex++;
        CharIndex = 0;
    }

    public void JoinLineUpwards()
    {
        if (LineIndex == 0)
            return;

        string lineContent = Line;
        DeleteLine();

        Insert(lineContent);
        CharIndex = LineLength - lineContent.Length;
    }

    public void JumpToEndOfLine()
    {
        CharIndex = LineLength;
    }

    public void JumpToStartOfLine()
    {
        CharIndex = 0;
    }

    public int GetJumpLeftLength()
    {
        return 2;
    }

    public int GetJumpRightLength()
    {
        return 2;
    }

    private void ClampCaretIfNecessary()
    {
        if (CharIndex > LineLength)
            CharIndex = LineLength;
    }
}
