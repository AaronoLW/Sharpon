using System.Numerics;
using System.Text;
using System.Xml.Serialization;
using KatziDrip;
using SDL3;
using SmashFramework;
using Color = System.Drawing.Color;

public class TextDocument(string text) : ITextContainer
{
    public List<string> Lines => _lines;
    private readonly List<string> _lines = [.. text.Split('\n')];

    public int LineCount => _lines.Count;

    public int PointSize { get; set; } = App.POINT_SIZE;

    // WHY IS THIS GIVING ME A WARNING FUCK YOU
#pragma warning disable CS9266
    public int LineSpacing { get => PointSize + App.LINE_SPACING_POINT_SIZE_INCREASE; set; }
#pragma warning restore CS9266

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

    public string SelectedLine
    {
        get => _lines[LineIndex];
        private set => _lines[LineIndex] = value;
    }

    public int LineLength => GetLineLength(LineIndex);

    public Vector2 GetCaretPosition(Vector2 offset)
    {
        float x = FontManager.FontRegular.MeasureString(SelectedLine[0..CharIndex], PointSize).X;
        float y = LineSpacing * LineIndex;

        return offset + new Vector2(x, y);
    }

    public void Insert(string text)
    {
        SelectedLine = SelectedLine.Insert(CharIndex, text);
        CharIndex += text.Length;

        if (CharIndex > LineLength)
            CharIndex--;
    }

    // hiiiiiii
    // hi but not

    public void Insert(string[] lines)
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

    public void Append(string text)
    {
        _lines.Add(text);
    }

    public void Append(string[] texts)
    {
        foreach (string line in texts)
            _lines.Add(line);
    }

    public void RemoveBackwards(int count)
    {
        if (TextHelper.IsEnclosedInBrackets(this))
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

    public void DeleteLine()
    {
        if (LineIndex == 0)
        {
            if (_lines.Count > 1)
            {
                _lines.RemoveAt(0);
                if (CharIndex > LineLength)
                    CharIndex = LineLength;
            }
            else
            {
                _lines[0] = "";
                CharIndex = 0;
            }
        }
        else
        {
            _lines.RemoveAt(LineIndex);
            if (LineIndex == _lines.Count)
            {
                LineIndex--;
            }
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

    public void InsertNewLine(bool fancyStuff = true)
    {
        string text = SelectedLine[CharIndex..SelectedLine.Length];

        if (!fancyStuff)
        {
            SelectedLine = SelectedLine[0..CharIndex];
            _lines.Insert(LineIndex + 1, text);
            LineIndex++;
            CharIndex = 0;
            return;
        }

        if (TextHelper.IsEnclosedInBrackets(this))
        {
            if (App.CSharpBracketStyle)
            {
                int indent = GetLineStartIndent();
                char bracket = SelectedLine[CharIndex - 1];

                CharIndex--;
                SelectedLine = SelectedLine[0..CharIndex];
                Lines.Insert(LineIndex + 1, TextHelper.GetSpacesString(indent) + text);
                Lines.Insert(LineIndex + 1, "");
                Lines.Insert(LineIndex + 1, TextHelper.GetSpacesString(indent) + bracket);
                LineIndex += 2;
                CharIndex = 0;
                Insert(TextHelper.GetSpacesString(indent + 4));
            }
            else
            {
                int indent = GetLineStartIndent();

                SelectedLine = SelectedLine[0..CharIndex];
                Lines.Insert(LineIndex + 1, TextHelper.GetSpacesString(indent) + text);
                Lines.Insert(LineIndex + 1, TextHelper.GetSpacesString(indent));
                LineIndex++;
                CharIndex = 0;
                Insert(TextHelper.GetSpacesString(indent + 4));
            }
        }
        else
        {
            int indent = GetLineStartIndent();

            SelectedLine = SelectedLine[0..CharIndex];
            Lines.Insert(LineIndex + 1, TextHelper.GetSpacesString(indent) + text);
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

        string lineContent = SelectedLine;
        DeleteLine();

        CharIndex = SelectedLine.Length;
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

    public int GetLineStartIndent()
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

    public void PasteClipboard()
    {
        string clipboardText = SDL.GetClipboardText();
        string[] lines = clipboardText.Split('\n');
        Insert(lines);
    }

    public void StartSelection(SelectionMode selectionMode)
    {
        SelectionMode = selectionMode;
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
                if (_lines.Count == 1)
                {
                    _lines[0] = "";
                    CharIndex = 0;
                    break;
                }

                _lines.RemoveAt(startLine);
            }

            LineIndex = startLine;

            if (LineIndex > _lines.Count - 1)
                LineIndex = _lines.Count - 1;

            ClampCaretIfNecessary();
        }
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

        if (SelectionMode == SelectionMode.Linewise)
        {
            if (IsLineInSelection(lineIndex))
            {
                return (0, Lines[lineIndex].Length);
            }
        }

        return null;
    }

    public void JumpToMousePosition(Vector2 offset)
    {
        Vector2 mousePosition = Input.MousePosition - offset;

        int lineIndex = (int)(mousePosition.Y / LineSpacing);
        int charIndex = 0;

        if (lineIndex < 0)
            return;

        if (lineIndex < _lines.Count)
        {
            charIndex = (int)(mousePosition.X / FontManager.FontRegular.MeasureString("W", PointSize).X);
        }
        else lineIndex = _lines.Count - 1;

        if (charIndex > GetLineLength(lineIndex))
            charIndex = GetLineLength(lineIndex);

        if (charIndex < 0)
            charIndex = 0;

        LineIndex = lineIndex;
        CharIndex = charIndex;
    }

    public string GetSelectedText()
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
                return _lines[startLine][0.._lines[startLine].Length];
            else
                return _lines[startLine][startChar..endChar];
        }

        for (int i = startLine; i <= endLine; i++)
        {
            if (i == startLine)
            {
                if (LineIndex < SelectionLineIndex)
                    sb.Append(_lines[i][CharIndex.._lines[i].Length]);
                else
                    sb.Append(_lines[i][0..CharIndex]);
            }
            else if (i == endLine)
            {
                if (LineIndex > SelectionLineIndex)
                    sb.Append(_lines[i][SelectionCharIndex.._lines[i].Length]);
                else
                    sb.Append(_lines[i][0..(SelectionCharIndex + 1)]);
            }
            else
            {
                sb.Append(_lines[i]);
            }

            if (i != _lines.Count - 1)
            {
                sb.Append('\n');
            }
        }

        return sb.ToString();
    }

    public void CopySelection()
    {
        if (SelectionMode == SelectionMode.None)
            return;

        SDL.SetClipboardText(GetSelectedText());
    }

    public void Clear()
    {
        _lines.Clear();
        CharIndex = 0;
        LineIndex = 0;
        _lines.Add("");
    }

    public void JumpToEndOfFile()
    {
        LineIndex = _lines.Count - 1;
        CharIndex = LineLength;
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

    private int GetLineLength(int lineIndex)
    {
        return Math.Max(_lines[lineIndex].Length - ((CaretStyle == CaretStyle.Block || CaretStyle == CaretStyle.Underline) ? 1 : 0), 0);
    }
}
