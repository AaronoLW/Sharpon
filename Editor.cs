using System.Numerics;
using System.Text;
using SDL3;
using SmashFramework;

using Color = System.Drawing.Color;

public class Editor(string initialText = "")
{
    public string Text { get; private set; } = initialText;
    public int CharIndex = initialText.Length;

    private static readonly Vector2 Offset = new(20);

    public void Update(double deltaTime)
    {
        StringBuilder stringBuilder = new(Text);
        if (Input.TextInput != null)
        {
            stringBuilder.Append(Input.TextInput);
            CharIndex += Input.TextInput.Length;
        }

        if (Input.IsKeyPressed(SDL.Keycode.Backspace))
        {
            if (CharIndex > 0)
            {
                stringBuilder.Remove(CharIndex - 1, 1);
                CharIndex--;
            }
        }

        if (Input.IsKeyPressed(SDL.Keycode.Return))
        {
            stringBuilder.Append('\n');
            CharIndex++;
        }

        if (Input.IsKeyPressed(SDL.Keycode.Left))
        {
            if (CharIndex > 0)
                CharIndex--;
        }

        if (Input.IsKeyPressed(SDL.Keycode.Right))
        {
            if (CharIndex < Text.Length)
                CharIndex++;
        }

        Text = stringBuilder.ToString();
    }

    public void Render(Renderer renderer)
    {
        renderer.RenderText(App.Font, App.POINT_SIZE, Text, Offset, Color.White);

        Rectangle caret = new(GetCaretPosition(), 2, App.POINT_SIZE);
        renderer.RenderFilledRectangle(caret, Color.RoyalBlue);
    }

    private Vector2 GetCaretPosition()
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

        return Offset + textSize;
    }

    //private int GetLineIndex()
    //{
    //    if (Text.Length == 0) return 0;

    //    int newLineCount = 0;
    //    for (int i = 0; i < CharIndex; i++)
    //    {
    //        if (Text[i] == '\n')
    //            newLineCount++;
    //    }

    //    return newLineCount;
    //}

    private string GetSelectedLineUntilCaret()
    {
        if (Text.Length == 0) return "";

        int startIndex = 0;
        for (int i = CharIndex - 1; i > 0; i--)
        {
            if (Text[i] == '\n')
            {
                startIndex = i;
                break;
            }
        }

        int endIndex = CharIndex;
        //for (int i = CharIndex; i < Text.Length; i++)
        //{
        //    if (Text[i] == '\n')
        //    {
        //        endIndex = i;
        //        break;
        //    }
        //}

        return Text[startIndex..endIndex];
    }
}
