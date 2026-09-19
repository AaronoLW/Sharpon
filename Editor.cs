using System.Numerics;
using System.Text;
using SDL3;
using SmashFramework;

using Color = System.Drawing.Color;

public class Editor(string initialText = "")
{
    private readonly TextDocument _document = new(initialText, initialText.Length);

    private static readonly Vector2 Offset = new(20);

    public void Update(double deltaTime)
    {
        if (Program.TextInput != null)
        {
            _document.Insert(Program.TextInput);
        }

        if (Program.IsKeyDown(SDL.Keycode.Backspace))
        {
            _document.TryRemoveBackwards(1);
        }

        if (Program.IsKeyDown(SDL.Keycode.Return))
        {
            _document.Insert('\n');
        }

        if (Program.IsKeyDown(SDL.Keycode.Left))
        {
            if (_document.CharIndex > 0)
                _document.CharIndex--;
        }

        if (Program.IsKeyDown(SDL.Keycode.Right))
        {
            if (_document.CharIndex < _document.Text.Length)
                _document.CharIndex++;
        }

        if (Program.IsKeyDown(SDL.Keycode.Tab))
        {
            _document.Insert("    ");
        }
    }

    public void Render(Renderer renderer)
    {
        renderer.RenderText(App.Font, App.POINT_SIZE, _document.Text, Offset, Color.White);

        Rectangle caret = new(_document.GetCaretPosition(Offset), 2, App.POINT_SIZE);
        renderer.RenderFilledRectangle(caret, Color.RoyalBlue);
    }
}
