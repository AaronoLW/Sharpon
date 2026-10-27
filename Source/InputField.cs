using System.Drawing;
using System.Numerics;
using SmashFramework;

public class InputField : ITextContainer
{
    public string Text { get; set; } = "";
    public string Prefix = "";

    public int CharIndex { get; private set; }
    public int LineIndex { get; private set; }

    public int SelectionCharIndex { get; private set; }
    public int SelectionLineIndex { get; private set; }

    public SelectionMode SelectionMode { get; private set; }

    public int LineCount => 1;

    public int PointSize { get; set; } = App.POINT_SIZE;

#pragma warning disable
    public int LineSpacing { get => PointSize + App.LINE_SPACING_POINT_SIZE_INCREASE; set; }
#pragma warning restore

    public int LineLength { get => GetLineLength(); }

    public string SelectedLine => Text;

    public float X;
    public float Y;
    public Vector2 Position
    {
        get => new(X, Y);
        set { X = value.X; Y = value.Y; }
    }

    public float Width;
    public float Height;
    public Vector2 Size
    {
        get => new(Width, Height);
        set { Width = value.X; Height = value.Y; }
    }

    private CaretStyle _caretStyle = CaretStyle.Beam;

    public CaretStyle CaretStyle
    {
        get => _caretStyle;
        set { _caretStyle = value; ClampCaretIfNecessary(); }
    }

    public void Render(Renderer renderer)
    {
        float xOffset = 0;

        if (Prefix != string.Empty)
        {
            Vector2 prefixTextSize = FontManager.FontRegular.MeasureString(Prefix, App.POINT_SIZE);
            Vector2 prefixPosition = Position + new Vector2(App.DEFAULT_PADDING, (Height / 2) - (prefixTextSize.Y / 2));
            renderer.RenderString(FontManager.FontRegular, App.POINT_SIZE, Prefix, Vector2.Round(prefixPosition), Color.White);
            xOffset += prefixTextSize.X;
        }

        Vector2 textSize = FontManager.FontRegular.MeasureString(Text, App.POINT_SIZE);
        Vector2 textPosition = Position + new Vector2(xOffset * 2, 0) + new Vector2(App.DEFAULT_PADDING, (Height / 2) - (textSize.Y / 2));
        renderer.RenderString(FontManager.FontRegular, App.POINT_SIZE, Text, Vector2.Round(textPosition), Color.White);
    }

    public void Insert(string text)
    {
        Text = Text.Insert(CharIndex, text);
        CharIndex += text.Length;
    }

    public void Insert(string[] texts)
    {
        string text = TextHelper.JoinStringArrayToSingleString(texts);
        Text = Text.Insert(CharIndex, text);
        CharIndex += text.Length;
    }

    public void Append(string text)
    {
        Text += text;
    }

    public void Append(string[] texts)
    {
        Text += TextHelper.JoinStringArrayToSingleString(texts);
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

        Text = Text.Remove(CharIndex - count, count);
        CharIndex -= count;
    }

    public void MoveUp()
    {
        CharIndex = 0;
    }

    public void MoveRight(int amount)
    {
        CharIndex += amount;

        if (CharIndex > LineLength)
            CharIndex = LineLength;
    }

    public void MoveLeft(int amount)
    {
        CharIndex -= amount;
        if (CharIndex < 0)
            CharIndex = 0;
    }

    public void MoveDown()
    {
        CharIndex = LineLength;
    }

    public void DeleteLine()
    {
        Clear();
    }

    public void Clear()
    {
        Text = "";
        CharIndex = 0;
    }

    public void DeleteSelection()
    {
        throw new NotImplementedException();
    }

    public void CopySelection()
    {
        throw new NotImplementedException();
    }

    public void EndSelection()
    {
        throw new NotImplementedException();
    }

    public string GetSelectedText()
    {
        throw new NotImplementedException();
    }

    public (int, int)? GetSelectionRangeOnLine(int lineIndex)
    {
        throw new NotImplementedException();
    }

    public void PasteClipboard()
    {
        throw new NotImplementedException();
    }

    public void StartSelection(SelectionMode selectionMode)
    {
        throw new NotImplementedException();
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

    public Vector2 GetCaretPosition(Vector2 offset)
    {
        float x = FontManager.FontRegular.MeasureString(Text[0..CharIndex], PointSize).X;
        float y = LineSpacing * LineIndex;

        return offset + new Vector2(x, y);
    }

    public void InsertTab()
    {
        Text = Text.Insert(CharIndex, TextHelper.GetSpacesString(4));
    }

    public void JumpToEndOfFile()
    {
        JumpToEndOfLine();
    }

    public void JumpToEndOfLine()
    {
        CharIndex = LineLength;
    }

    public void JumpToStartOfLine()
    {
        CharIndex = 0;
    }

    public void JumpToMousePosition(Vector2 offset)
    {
        Vector2 mousePosition = Input.MousePosition - offset;

        int lineIndex = (int)(mousePosition.Y / LineSpacing);
        int charIndex = 0;

        if (lineIndex < 0)
            return;

        if (lineIndex > 0)
        {
            charIndex = (int)(mousePosition.X / FontManager.FontRegular.MeasureString("W", PointSize).X);
        }

        if (charIndex > GetLineLength())
            charIndex = GetLineLength();

        if (charIndex < 0)
            charIndex = 0;

        lineIndex = 0;

        LineIndex = lineIndex;
        CharIndex = charIndex;
    }

    public void InsertNewLine(bool doFancyStuff = false) { }
    public void InsertNewLineOnPreviousLine() { }
    public void JoinLineUpwards() { }

    private void ClampCaretIfNecessary()
    {
        if (CharIndex > LineLength)
            CharIndex = LineLength;
    }

    private int GetLineLength()
    {
        return Math.Max(Text.Length - ((CaretStyle == CaretStyle.Block || CaretStyle == CaretStyle.Underline) ? 1 : 0), 0);
    }
}
