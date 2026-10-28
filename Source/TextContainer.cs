using System.Numerics;
using System.Text;
using SDL3;
using SmashFramework;

public abstract class TextContainer
{
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

    public const int MIN_POINT_SIZE = 4;

    public int PointSize { get; set; } = App.POINT_SIZE;

    // WHY IS THIS GIVING ME A WARNING FUCK YOU
#pragma warning disable CS9266
    public int LineSpacing { get => PointSize + App.LINE_SPACING_POINT_SIZE_INCREASE; set; }
#pragma warning restore CS9266

    public int CharIndex { get; protected set; }
    public int LineIndex { get; protected set; }

    public List<string> Lines { get; protected set; } = [""];
    public string SelectedLine
    {
        get => Lines[LineIndex];
        set => Lines[LineIndex] = value;
    }

    public int LineCount => Lines.Count;

    public int LineLength => GetLineLength(LineIndex);

    public SelectionMode SelectionMode { get; protected set; }
    public int SelectionCharIndex { get; protected set; }
    public int SelectionLineIndex { get; protected set; }

    public virtual CaretStyle CaretStyle { get; set; }

    public abstract string Text { get; }

    public virtual void Insert(string text)
    {
        SelectedLine = SelectedLine.Insert(CharIndex, text);
        CharIndex += text.Length;

        ClampCaretIfNecessary();
    }

    public virtual void Insert(string[] lines)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            SelectedLine = SelectedLine.Insert(CharIndex, lines[i]);
            CharIndex += lines[i].Length;

            if (i != lines.Length - 1)
                InsertNewLine(fancyStuff: false);
        }

        ClampCaretIfNecessary();
    }

    public virtual void Append(string text)
    {
        Append(text.Split('\n'));
    }

    public virtual void Append(string[] texts)
    {
        foreach (string line in texts)
            Lines.Add(line);
    }

    public virtual void MoveLeft(int count = 1)
    {
        if (CharIndex - (count - 1) > 0)
            CharIndex -= count;
    }

    public virtual void MoveRight(int count = 1)
    {
        if (CharIndex + (count - 1) < LineLength)
            CharIndex += count;
    }

    public virtual void MoveUp()
    {
        if (LineIndex > 0)
        {
            LineIndex--;
            ClampCaretIfNecessary();
        }

    }

    public virtual void MoveDown()
    {
        if (LineIndex < Lines.Count - 1)
        {
            LineIndex++;
            ClampCaretIfNecessary();
        }
    }

    public virtual void RemoveBackwards(int count)
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

        SelectedLine = SelectedLine.Remove(CharIndex - count, count);
        CharIndex -= count;
    }

    public virtual void DeleteLine()
    {
        if (LineIndex == 0)
        {
            if (Lines.Count > 1)
            {
                Lines.RemoveAt(0);
                if (CharIndex > LineLength)
                    CharIndex = LineLength;
            }
            else
            {
                Lines[0] = "";
                CharIndex = 0;
            }
        }
        else
        {
            Lines.RemoveAt(LineIndex);
            if (LineIndex == LineCount)
            {
                LineIndex--;
            }
        }
    }

    public virtual void JoinLineUpwards()
    {
        if (LineIndex == 0)
            return;

        string lineContent = SelectedLine;
        DeleteLine();

        CharIndex = SelectedLine.Length;
        Insert(lineContent);
        CharIndex = LineLength - lineContent.Length;
    }


    public virtual void InsertNewLine(bool fancyStuff = true)
    {
        string text = SelectedLine[CharIndex..SelectedLine.Length];

        if (!fancyStuff)
        {
            SelectedLine = SelectedLine[0..CharIndex];
            Lines.Insert(LineIndex + 1, text);
            LineIndex++;
            CharIndex = 0;
            return;
        }

        if (IsEnclosedInBrackets())
        {
            if (App.CSharpBracketStyle)
            {
                int indent = GetLineStartIndent();
                char bracket = SelectedLine[CharIndex - 1];

                CharIndex--;
                SelectedLine = SelectedLine[0..CharIndex];
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

                SelectedLine = SelectedLine[0..CharIndex];
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

            SelectedLine = SelectedLine[0..CharIndex];
            Lines.Insert(LineIndex + 1, GetSpacesString(indent) + text);
            LineIndex++;
            CharIndex = indent;
        }
    }

    public virtual void InsertNewLineOnPreviousLine()
    {
        if (LineIndex > 0)
        {
            LineIndex--;
            CharIndex = LineLength;
            InsertNewLine();
        }
        else
        {
            Lines.Insert(0, "");
            CharIndex = 0;
        }
    }

    public virtual void InsertTab()
    {
        Insert("    ");
    }

    public virtual void JumpToEndOfLine()
    {
        CharIndex = LineLength;
    }

    public virtual void JumpToStartOfLine()
    {
        CharIndex = 0;
    }

    public virtual void PasteClipboard()
    {
        string clipboardText = SDL.GetClipboardText();
        string[] lines = clipboardText.Split('\n');
        Insert(lines);
    }

    public virtual void StartSelection(SelectionMode selectionMode)
    {
        SelectionMode = selectionMode;
        SelectionCharIndex = CharIndex;
        SelectionLineIndex = LineIndex;
    }

    public virtual void EndSelection()
    {
        SelectionMode = SelectionMode.None;
        SelectionCharIndex = 0;
        SelectionLineIndex = 0;
    }


    public virtual int GetLineStartIndent()
    {
        if (LineLength == 0)
            return 0;

        int index = 0;

        while (SelectedLine[index] == ' ')
        {
            index++;

            if (index == LineLength)
                break;
        }

        return index;
    }


    public virtual Vector2 GetCaretPosition(Vector2 offset)
    {
        float x = FontManager.FontRegular.MeasureString(SelectedLine[0..CharIndex], PointSize).X;
        float y = LineSpacing * LineIndex;

        return offset + new Vector2(x, y);
    }

    public bool IsEnclosedInBrackets()
    {
        if (CharIndex >= LineLength)
            return false;

        if (CharIndex == 0)
            return false;

        if (BracketsClosed.Contains(SelectedLine[CharIndex]) &&
            BracketsOpen.Contains(SelectedLine[CharIndex - 1]))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Automatically adjusts the CharIndex and LineIndex to not be out of bounds
    /// </summary>
    public virtual void DeleteSelection()
    {
        // Aaron = Katze
        // Katze = gut
        // Katze.Coding = peak
        // Katze.Coding = funktioniert

        if (SelectionMode == SelectionMode.None)
            return;

        if (SelectionMode == SelectionMode.Default)
        {
            int startLine = Math.Min(LineIndex, SelectionLineIndex);
            int endLine = Math.Max(LineIndex, SelectionLineIndex);

            if (startLine == endLine)
            {
                int startChar = Math.Min(CharIndex, SelectionCharIndex);
                int endChar = Math.Max(CharIndex, SelectionCharIndex);

                SelectedLine = SelectedLine.Remove(startChar, endChar - startChar + 1);
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
                    SelectedLine += Lines[LineIndex + 1];
                    Lines.RemoveAt(LineIndex + 1);
                }
                else
                {
                    LineIndex--;
                    CharIndex = LineLength;
                    SelectedLine += Lines[LineIndex + 1];
                    Lines.RemoveAt(LineIndex + 1);
                    CharIndex++;
                }
            }

            // I now know why I never implemented text selection
        }
        else if (SelectionMode == SelectionMode.Linewise)
        {
            int startLine = Math.Min(LineIndex, SelectionLineIndex);
            int endLine = Math.Max(LineIndex, SelectionLineIndex);


            for (int i = 0; i <= endLine - startLine; i++)
            {
                if (Lines.Count == 1)
                {
                    Lines[0] = "";
                    CharIndex = 0;
                    break;
                }

                Lines.RemoveAt(startLine);
            }

            LineIndex = startLine;

            if (LineIndex > Lines.Count - 1)
                LineIndex = Lines.Count - 1;

            ClampCaretIfNecessary();
        }
    }

    /// <summary>
    /// (from, to)
    /// </summary
    public virtual (int, int)? GetSelectionRangeOnLine(int lineIndex)
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

        if (SelectionMode == SelectionMode.Linewise)
        {
            if (IsLineInSelection(lineIndex))
            {
                return (0, Lines[lineIndex].Length);
            }
        }

        return null;
    }

    public virtual void JumpToMousePosition(Vector2 offset)
    {
        Vector2 mousePosition = Input.MousePosition - offset;

        int lineIndex = (int)(mousePosition.Y / LineSpacing);
        int charIndex = 0;

        if (lineIndex < 0)
            return;

        if (lineIndex < Lines.Count)
        {
            charIndex = (int)(mousePosition.X / FontManager.FontRegular.MeasureString("W", PointSize).X);
        }
        else lineIndex = Lines.Count - 1;

        if (charIndex > GetLineLength(lineIndex))
            charIndex = GetLineLength(lineIndex);

        if (charIndex < 0)
            charIndex = 0;

        LineIndex = lineIndex;
        CharIndex = charIndex;
    }

    public virtual string GetSelectedText()
    {
        StringBuilder sb = new();

        int startLine = Math.Min(LineIndex, SelectionLineIndex);
        int endLine = Math.Max(LineIndex, SelectionLineIndex);

        // #coding

        if (startLine == endLine)
        {
            int startChar = Math.Min(CharIndex, SelectionCharIndex);
            int endChar = Math.Max(CharIndex, SelectionCharIndex);

            if (SelectionMode == SelectionMode.Linewise)
                return Lines[startLine][0..Lines[startLine].Length];
            else
                return Lines[startLine][startChar..endChar];
        }

        for (int i = startLine; i <= endLine; i++)
        {
            if (i == startLine)
            {
                if (LineIndex < SelectionLineIndex)
                    sb.Append(Lines[i][CharIndex..Lines[i].Length]);
                else
                    sb.Append(Lines[i][0..CharIndex]);
            }
            else if (i == endLine)
            {
                if (LineIndex > SelectionLineIndex)
                    sb.Append(Lines[i][SelectionCharIndex..Lines[i].Length]);
                else
                    sb.Append(Lines[i][0..(SelectionCharIndex + 1)]);
            }
            else
            {
                sb.Append(Lines[i]);
            }

            if (i != Lines.Count - 1)
            {
                sb.Append('\n');
            }
        }

        return sb.ToString();
    }

    public virtual void CopySelection()
    {
        if (SelectionMode == SelectionMode.None)
            return;

        SDL.SetClipboardText(GetSelectedText());
    }

    public virtual void Clear()
    {
        Lines.Clear();
        CharIndex = 0;
        LineIndex = 0;
        Lines.Add("");
    }

    public virtual void JumpToEndOfFile()
    {
        LineIndex = Lines.Count - 1;
        CharIndex = LineLength;
    }

    public virtual int GetJumpLeftLength()
    {
        if (CharIndex == 0)
        {
            if (LineIndex > 0)
                return 1;
            else
                return 0;
        }

        if (IsSpecialCharacter(SelectedLine[CharIndex - 1]))
        {
            if (IsEnclosedInBrackets())
            {
                MoveRight();
                return 2;
            }
            else return 1;
        }

        bool foundWord = true;
        if (char.IsWhiteSpace(SelectedLine[CharIndex - 1]))
            foundWord = false;

        int length = 0;
        for (int i = CharIndex - 1; i > 0; i--)
        {
            if (IsSpecialOrWhiteSpaceCharacter(SelectedLine[i]))
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

    public virtual int GetJumpRightLength()
    {
        if (CharIndex == LineLength)
        {
            if (LineIndex == LineCount)
                return 0;
            else
                return 1;
        }

        if (IsSpecialCharacter(SelectedLine[CharIndex]))
            return 1;

        bool foundWord = true;
        if (char.IsWhiteSpace(SelectedLine[CharIndex]))
            foundWord = false;

        int length = 0;
        for (int i = CharIndex; i < LineLength; i++)
        {
            if (IsSpecialOrWhiteSpaceCharacter(SelectedLine[i]))
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

    protected int GetLineLength(int lineIndex)
    {
        return Math.Max(Lines[lineIndex].Length - ((CaretStyle == CaretStyle.Block || CaretStyle == CaretStyle.Underline) ? 1 : 0), 0);
    }

    protected void ClampCaretIfNecessary()
    {
        if (CharIndex > LineLength)
            CharIndex = LineLength;
    }

    protected bool IsLineInSelection(int lineIndex)
    {
        if (LineIndex <= lineIndex && SelectionLineIndex >= lineIndex ||
            SelectionLineIndex <= lineIndex && LineIndex >= lineIndex)
            return true;

        return false;
    }

    protected string GetSpacesString(int amount)
    {
        StringBuilder fuckyou = new();
        for (int i = 0; i < amount; i++)
        {
            fuckyou.Append(' ');
        }

        return fuckyou.ToString();
    }

    protected bool IsSpecialCharacter(char character)
    {
        return BracketsOpen.Contains(character) ||
               BracketsClosed.Contains(character) ||
               SpecialCharacters.Contains(character);
    }

    protected bool IsSpecialOrWhiteSpaceCharacter(char character)
    {
        return BracketsOpen.Contains(character) ||
               BracketsClosed.Contains(character) ||
               SpecialCharacters.Contains(character) ||
               character == ' ';
    }
}
