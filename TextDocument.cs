using System.Text;
using System.Numerics;

public class TextDocument(string? initialText, int initialCharIndex = 0)
{
    private const string DEFAULT_TEXT = "Text der zum testen gedacht ist (Raphi edition)\n djkawdjawdkjawdjkawdawjdawjdklawjdaklwdjawkjdjawkldjawkldjawkldjaw";

    public string Text => _stringBuilder.ToString();
    public int CharIndex = initialCharIndex;

    public int LineCharIndex => CharIndex - GetLineStartIndex();

    private readonly StringBuilder _stringBuilder = new(initialText ?? DEFAULT_TEXT);

    public void Insert(string text)
    {
        _stringBuilder.Insert(CharIndex, text);
        CharIndex += text.Length;
    }

    public void Insert(char text)
    {
        _stringBuilder.Insert(CharIndex, text);
        CharIndex += 1;
    }

    public void TryRemoveBackwards(int length)
    {
        if (CharIndex > length - 1)
        {
            _stringBuilder.Remove(CharIndex - length, length);
            CharIndex -= length;
        }
    }

    public Vector2 GetCaretPosition(Vector2 offset, EditorStyle style)
    {
        int lineCount = Text.AsSpan()[0..CharIndex].Count('\n');
        Vector2 textSize = new(0, style.LineSpacing * lineCount);

        string selectedLine = GetSelectedLineUntilCaret();
        textSize.X = App.Font.MeasureString(selectedLine, style.PointSize).X;

        if (selectedLine == "\n")
        {
            textSize.X -= App.Font.MeasureString("\n", style.PointSize).X;
        }

        return offset + textSize;
    }

    public int FindLengthOfLastWord()
    {
        if (CharIndex == 0)
            return 0;

        bool word = true;

        if (Text[CharIndex - 1] == '\n')
            return 1;

        if (char.IsWhiteSpace(Text[CharIndex - 1]))
            word = false;

        int skipAmount = 0;

        if (CharIndex > 1 && !char.IsWhiteSpace(Text[CharIndex - 2]))
        {
            word = true;
            skipAmount++;
        }


        int length = 0;
        for (int i = CharIndex; i > 1; i--)
        {
            if (skipAmount > 0)
            {
                skipAmount--;
                length++;
                continue;
            }

            if (char.IsWhiteSpace(Text[i - 1]) && word)
            {
                return length;
            }
            else if (!char.IsWhiteSpace(Text[i - 1]) && !word)
            {
                return length;
            }

            length++;
        }

        return CharIndex;
    }

    public void MoveUp()
    {
        int startIndex = LineCharIndex;

        if (startIndex == CharIndex)
        {
            CharIndex = 0;
            return;
        }

        CharIndex -= startIndex + 1;

        if (GetLineStartIndex() + startIndex > GetLineStartIndex() + GetLineLength())
        {
            CharIndex = GetLineStartIndex() + GetLineLength();
            return;
        }

        CharIndex = GetLineStartIndex() + startIndex;
    }

    public void MoveDown()
    {
        int startIndex = LineCharIndex;

        CharIndex = GetLineStartIndex() + GetLineLength();

        if (CharIndex == Text.Length)
            return;

        CharIndex++;
        CharIndex += startIndex;

        if (CharIndex > Text.Length)
            CharIndex = Text.Length;
    }

    private int GetLineLength()
    {
        int startIndex = GetLineStartIndex();

        int length = 0;
        for (int i = startIndex; i < Text.Length; i++)
        {
            if (Text[i] == '\n')
            {
                return length;
            }

            length++;
        }

        return length;
    }

    private string GetSelectedLineUntilCaret()
    {
        if (Text.Length == 0) return "";

        int startIndex = GetLineStartIndex();

        return Text[startIndex..CharIndex];
    }

    private int GetLineStartIndex()
    {
        int startIndex = 0;
        for (int i = CharIndex - 1; i > 0; i--)
        {
            if (Text[i] == '\n')
            {
                startIndex = i + 1;
                break;
            }
        }

        return startIndex;
    }

    public static TextDocument FromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Couldn't find file at {filePath}");
        }

        string fileContent = File.ReadAllText(filePath);
        return new(fileContent, 0);
    }
}
