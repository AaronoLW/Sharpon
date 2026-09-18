using System.Numerics;
using System.Text;
using SDL3;
using SmashFramework;

using Color = System.Drawing.Color;

public class Editor(string initialText = "")
{
    public string Text { get; private set; } = initialText;
    public int CharIndex = 0;

    private static readonly Vector2 Offset = new(20);

    public void Update(double deltaTime)
    {
        StringBuilder stringBuilder = new(Text);
        if (Input.TextInput != null)
        {
            stringBuilder.Append(Input.TextInput);
        }

        if (Input.IsKeyPressed(SDL.Keycode.Backspace))
        {
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
        }

        if (Input.IsKeyPressed(SDL.Keycode.Return))
        {
            stringBuilder.Append('\n');
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
        Vector2 textSize = App.Font.MeasureString(Text, App.POINT_SIZE);

        if (Text.Length > 0)
            textSize.Y -= App.Font.MeasureString("W", App.POINT_SIZE).Y;

        return Offset + textSize;
    }
}
