using System.Numerics;
using SDL3;

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
        CharIndex += text.Length;

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

    public void InsertNewLineOnPreviousLine()
    {
        if (LineIndex > 0)
        {
            LineIndex--;
            CharIndex = LineLength;
        }

        InsertNewLine();
    }

    public void InsertTab()
    {
        Insert("    ");
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
        if (CharIndex == 0)
        {
            if (LineIndex > 0)
                return 1;
            else
                return 0;
        }

        bool foundWord = true;
        if (char.IsWhiteSpace(Line[CharIndex - 1]))
            foundWord = false;

        int length = 0;
        for (int i = CharIndex - 1; i > 0; i--)
        {
            if (Line[i] == ' ')
            {
                if (foundWord)
                    return length;
            }
            else
            {
                foundWord = true;
            }

            length++;
        }

        return CharIndex;
    }

    public int GetJumpRightLength()
    {
        if (CharIndex == LineLength)
        {
            if (LineIndex == Lines.Count)
                return 0;
            else
                return 1;
        }

        bool foundWord = true;
        if (char.IsWhiteSpace(Line[CharIndex]))
            foundWord = false;

        int length = 0;
        for (int i = CharIndex; i < LineLength; i++)
        {
            if (char.IsWhiteSpace(Line[i]))
            {
                if (foundWord)
                    return length;
            }
            else
            {
                foundWord = true;
            }

            length++;
        }

        return length;
    }

    public void PasteClipboard()
    {
        Insert(SDL.GetClipboardText());
    }

    private void ClampCaretIfNecessary()
    {
        if (CharIndex > LineLength)
            CharIndex = LineLength;
    }
}
