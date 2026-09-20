using System.Text;
using System.Numerics;

public class TextDocument(string initialText = "", int initialCharIndex = 0)
{
    public string Text => _stringBuilder.ToString();
    public int CharIndex = initialCharIndex;

    private readonly StringBuilder _stringBuilder = new(initialText);

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

    public Vector2 GetCaretPosition(Vector2 offset)
    {
        Vector2 textSize = new(0, App.Font.MeasureString(Text[0..CharIndex], App.POINT_SIZE).Y);

        if (CharIndex > 0 && Text[CharIndex - 1] != '\n')
        {
            textSize.Y -= App.Font.MeasureString("W", App.POINT_SIZE).Y;
        }

        string selectedLine = GetSelectedLineUntilCaret();
        textSize.X = App.Font.MeasureString(selectedLine, App.POINT_SIZE).X;

        if (selectedLine == "\n")
        {
            textSize.X -= App.Font.MeasureString("\n", App.POINT_SIZE).X;
        }

        return offset + textSize;
    }

    private string GetSelectedLineUntilCaret()
    {
        if (Text.Length == 0) return "";

        int startIndex = 0;
        for (int i = CharIndex - 1; i > 0; i--)
        {
            if (Text[i] == '\n')
            {
                startIndex = i + 1;
                break;
            }
        }

        return Text[startIndex..CharIndex];
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
