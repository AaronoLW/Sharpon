using System.Numerics;
using System.Text;
using SDL3;
using SmashFramework;

public class TextDocument(string text)
{
    public List<string> Lines => _lines;
    private readonly List<string> _lines = [.. text.Split('\n')];

    public static readonly char[] BracketsOpen = [
        '{',
        '(',
        '[',
        '"',
        '<',
    ];

    public static readonly char[] BracketsClosed = [
        '}',
        ')',
        ']',
        '"',
        '>',
    ];

    public static readonly char[] SpecialCharacters = [
        '.',
        ',',
        '/',
        '\\',
    ];

    public string Text => string.Join("\n", _lines);

    public int CharIndex { get; private set; }
    public int LineIndex { get; private set; }

    public SelectionMode SelectionMode { get; private set; } = SelectionMode.None;
    public int SelectionCharIndex { get; private set; } = 0;
    public int SelectionLineIndex { get; private set; } = 0;

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

    public int LineLength => GetLineLength(LineIndex);

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
        if (IsEnclosedInBrackets())
        {
            CharIndex++;
            count++;
        }

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
            CharIndex = 0;
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

        if (IsEnclosedInBrackets())
        {
            if (App.CSharpBracketStyle)
            {
                int indent = GetLineStartIndent();
                char bracket = Line[CharIndex - 1];

                CharIndex--;
                Line = Line[0..CharIndex];
                Lines.Insert(LineIndex + 1, GetSpacesString(indent) + text);
                Lines.Insert(LineIndex + 1, "");
                Lines.Insert(LineIndex + 1, GetSpacesString(indent) + bracket);
                LineIndex += 2;
                CharIndex = 0;
                Insert(GetSpacesString(indent + 4));
            }
            else
            {
                int indent = GetLineStartIndent();

                Line = Line[0..CharIndex];
                Lines.Insert(LineIndex + 1, GetSpacesString(indent) + text);
                Lines.Insert(LineIndex + 1, GetSpacesString(indent));
                LineIndex++;
                CharIndex = 0;
                Insert(GetSpacesString(indent + 4));
            }
        }
        else
        {
            int indent = GetLineStartIndent();

            Line = Line[0..CharIndex];
            Lines.Insert(LineIndex + 1, GetSpacesString(indent) + text);
            LineIndex++;
            CharIndex = indent;
        }
    }

    public void InsertNewLineOnPreviousLine()
    {
        if (LineIndex > 0)
        {
            LineIndex--;
            CharIndex = LineLength;
            InsertNewLine();
        }
        else
        {
            _lines.Insert(0, "");
            CharIndex = 0;
        }

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

        CharIndex = Line.Length;
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

        if (IsSpecialCharacter(Line[CharIndex - 1]))
        {
            if (IsEnclosedInBrackets())
            {
                CharIndex++;
                return 2;
            }
            else return 1;
        }

        bool foundWord = true;
        if (char.IsWhiteSpace(Line[CharIndex - 1]))
            foundWord = false;

        int length = 0;
        for (int i = CharIndex - 1; i > 0; i--)
        {
            if (IsSpecialOrWhiteSpaceCharacter(Line[i]))
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

        if (IsSpecialCharacter(Line[CharIndex]))
            return 1;

        bool foundWord = true;
        if (char.IsWhiteSpace(Line[CharIndex]))
            foundWord = false;

        int length = 0;
        for (int i = CharIndex; i < LineLength; i++)
        {
            if (IsSpecialOrWhiteSpaceCharacter(Line[i]))
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

    public int GetLineStartIndent()
    {
        if (LineLength == 0)
            return 0;

        int index = 0;

        while (Line[index] == ' ')
        {
            index++;

            if (index == LineLength)
                break;
        }

        return index;
    }

    public void PasteClipboard()
    {
        Insert(SDL.GetClipboardText());
    }

    public void StartSelection()
    {
        SelectionMode = SelectionMode.Default;
        SelectionCharIndex = CharIndex;
        SelectionLineIndex = LineIndex;
    }

    public void EndSelection()
    {
        SelectionMode = SelectionMode.None;
        SelectionCharIndex = 0;
        SelectionLineIndex = 0;
    }

    /// <summary>
    /// Automatically adjusts the CharIndex and LineIndex to not be out of bounds
    /// </summary>
    public void DeleteSelection()
    {
        // Aaron = Katze
        // Katze = gut
        // Katze.Coding = peak
        // Katze.Coding = funktioniert

        if (SelectionMode == SelectionMode.None)
            return;

        int startLine = Math.Min(LineIndex, SelectionLineIndex);
        int endLine = Math.Max(LineIndex, SelectionLineIndex);

        if (startLine == endLine)
        {
            int startChar = Math.Min(CharIndex, SelectionCharIndex);
            int endChar = Math.Max(CharIndex, SelectionCharIndex);

            Line = Line.Remove(startChar, endChar - startChar + 1);
            if (CharIndex > SelectionCharIndex)
            {
                CharIndex -= endChar - startChar + 1;
            }
        }
        else
        {
            for (int i = startLine; i <= endLine; i++)
            {
                if (i == startLine)
                {
                    Lines[i] = Lines[i].Remove(CharIndex, Lines[i].Length - CharIndex);
                    continue;
                }

                if (i == endLine)
                {
                    Lines[i] = Lines[i].Remove(0, SelectionCharIndex + 1);
                    continue;
                }

                Lines.RemoveAt(i);

                if (LineIndex > SelectionLineIndex)
                    LineIndex--;

                i--;
                endLine--;
            }

            if (SelectionLineIndex > LineIndex)
            {
                Line += Lines[LineIndex + 1];
                Lines.RemoveAt(LineIndex + 1);
            }
            else
            {
                LineIndex--;
                CharIndex = LineLength;
                Line += Lines[LineIndex + 1];
                Lines.RemoveAt(LineIndex + 1);
                CharIndex++;
            }
        }

        // I now know why I never implemented text selection
    }

    /// <summary>
    /// (from, to)
    /// </summary
    public (int, int)? GetSelectionRangeOnLine(int lineIndex)
    {
        if (SelectionMode == SelectionMode.None)
            return null;

        if (SelectionMode == SelectionMode.Default)
        {
            // Im losing my mind why am I doing this
            if (IsLineInSelection(lineIndex))
            {
                if (lineIndex == LineIndex)
                {
                    if (lineIndex < SelectionLineIndex)
                        return (CharIndex, Lines[lineIndex].Length);
                    else if (lineIndex > SelectionLineIndex)
                        return (0, CharIndex);
                    else
                    {
                        if (CharIndex < SelectionCharIndex)
                            return (SelectionCharIndex + 1, CharIndex);
                        else
                            return (SelectionCharIndex, CharIndex);
                    }
                }
                else
                {
                    if (lineIndex == SelectionLineIndex)
                    {
                        if (lineIndex < LineIndex)
                            return (SelectionCharIndex, Lines[lineIndex].Length);
                        else
                            return (0, SelectionCharIndex + 1);
                    }
                    else
                        return (0, Lines[lineIndex].Length);
                }
            }
            //else
            //{
            //  fucking die
            //}
        }

        return null;
    }

    public void JumpToMousePosition(Vector2 offset, float pointSize, float lineSpacing)
    {
        Vector2 mousePosition = Input.MousePosition - offset;

        int lineIndex = (int)(mousePosition.Y / lineSpacing);
        int charIndex = 0;

        if (lineIndex < _lines.Count)
        {
            charIndex = (int)(mousePosition.X / App.Font.MeasureString("W", pointSize).X);
        }
        else lineIndex = _lines.Count - 1;

        if (charIndex > GetLineLength(lineIndex))
            charIndex = GetLineLength(lineIndex);

        LineIndex = lineIndex;
        CharIndex = charIndex;
    }

    private bool IsLineInSelection(int lineIndex)
    {
        if (LineIndex <= lineIndex && SelectionLineIndex >= lineIndex ||
            SelectionLineIndex <= lineIndex && LineIndex >= lineIndex)
            return true;

        return false;
    }

    private void ClampCaretIfNecessary()
    {
        if (CharIndex > LineLength)
            CharIndex = LineLength;
    }

    private bool IsSpecialCharacter(char character)
    {
        return BracketsOpen.Contains(character) ||
               BracketsClosed.Contains(character) ||
               SpecialCharacters.Contains(character);
    }

    private int GetLineLength(int lineIndex)
    {
        return Math.Max(_lines[lineIndex].Length - ((CaretStyle == CaretStyle.Block || CaretStyle == CaretStyle.Underline) ? 1 : 0), 0);
    }

    private bool IsSpecialOrWhiteSpaceCharacter(char character)
    {
        return BracketsOpen.Contains(character) ||
               BracketsClosed.Contains(character) ||
               SpecialCharacters.Contains(character) ||
               character == ' ';
    }

    private bool IsEnclosedInBrackets()
    {
        if (CharIndex >= LineLength)
            return false;

        if (CharIndex == 0)
            return false;

        if (BracketsClosed.Contains(Line[CharIndex]) &&
            BracketsOpen.Contains(Line[CharIndex - 1]))
        {
            return true;
        }

        return false;
    }

    private string GetSpacesString(int amount)
    {
        StringBuilder fuckyou = new();
        for (int i = 0; i < amount; i++)
        {
            fuckyou.Append(' ');
        }

        return fuckyou.ToString();
    }
}
