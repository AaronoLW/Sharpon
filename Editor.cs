using System.Numerics;
using SDL3;
using SmashFramework;

using Color = System.Drawing.Color;

public class Editor
{
    private const float CARET_SPEED = 40f;

    private readonly TextDocument _document;
    private readonly FileManager _fileManager;

    private readonly Vector2 Offset = new(App.DEFAULT_PADDING);

    private Vector2 _caretPosition = new();

    public Editor(string? initialFilePath)
    {
        Console.WriteLine($"Trying to open initial file at: {initialFilePath ?? "null"}");

        _fileManager = new();
        _document = _fileManager.OpenFile(initialFilePath);
    }

    public void Update(double deltaTime)
    {
        if (Input.IsKeyDown(SDL.Keycode.LCtrl))
        {
            if (Program.IsKeyDown(SDL.Keycode.S))
            {
                _fileManager.SaveFile(_document);
            }

            if (Program.IsKeyDown(SDL.Keycode.Backspace))
            {
                _document.TryRemoveBackwards(_document.FindLengthOfLastWord());
            }

            if (Program.IsKeyDown(SDL.Keycode.Left))
            {
                _document.CharIndex -= _document.FindLengthOfLastWord();
            }
        }
        else
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

        Vector2 caretPosition = _document.GetCaretPosition(Offset);
        if (_caretPosition != caretPosition)
        {
            _caretPosition = MathHelper.LarpVector(_caretPosition, caretPosition, CARET_SPEED * (float)deltaTime);
        }
    }

    public void Render(Renderer renderer)
    {
        renderer.RenderText(App.Font, App.POINT_SIZE, _document.Text, Offset, Color.White);

        Vector2 filePathTextSize = App.Font.MeasureString(_fileManager.DisplayPath, App.POINT_SIZE);
        Vector2 filePathTextPosition = new(App.WindowWidth - filePathTextSize.X - App.DEFAULT_PADDING, App.DEFAULT_PADDING);
        renderer.RenderText(App.Font, App.POINT_SIZE, _fileManager.DisplayPath, filePathTextPosition, Color.White);

        Rectangle caret = new(_caretPosition, 2, App.POINT_SIZE);
        renderer.RenderFilledRectangle(caret, Color.RoyalBlue);
    }
}
