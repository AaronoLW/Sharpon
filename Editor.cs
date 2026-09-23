using System.Numerics;
using SDL3;
using SmashFramework;

using Color = System.Drawing.Color;

public class Editor
{
    private const float CARET_SPEED = 40f;
    private const int TOP_BAR_HEIGHT = 50;

    private const int MIN_POINT_SIZE = 4;

    private const int LINE_SPACING_POINT_SIZE_INCREASE = 6;

    private readonly TextDocument _document;
    private readonly FileManager _fileManager;
    private readonly KeybindHandler _keybindHandler = new();

    private readonly Vector2 Position = new(App.DEFAULT_PADDING, TOP_BAR_HEIGHT + App.DEFAULT_PADDING);

    private int _pointSize = App.POINT_SIZE;
    private float _lineSpacing => _pointSize + LINE_SPACING_POINT_SIZE_INCREASE;

    private EditorStyle _editorStyle => new(_pointSize, _lineSpacing);

    private Vector2 _caretPosition = new();

    public Editor(string? initialFilePath)
    {
        _fileManager = new();
        _document = _fileManager.OpenFile(initialFilePath);
    }

    public void Update(double deltaTime)
    {
        if (Program.TextInput != null)
        {
            _document.Insert(Program.TextInput);

            char? insert = null;
            if (Program.TextInput == "{") insert = '}';
            if (Program.TextInput == "(") insert = ')';
            if (Program.TextInput == "\"") insert = '"';
            if (Program.TextInput == "[") insert = ']';

            if (insert != null)
            {
                _document.Insert((char)insert);
                _document.CharIndex--;
            }
        }

        _keybindHandler.HandleKeybinds(_document);

        if (Input.IsKeyDown(SDL.Keycode.LCtrl))
        {
            if (Program.IsKeyDown(SDL.Keycode.S))
            {
                _fileManager.SaveFile(_document);
            }

            if (Program.IsKeyDown(SDL.Keycode.Plus))
            {
                _pointSize++;
            }

            if (Program.IsKeyDown(SDL.Keycode.Minus))
            {
                if (_pointSize > MIN_POINT_SIZE)
                    _pointSize--;
            }
        }

        Vector2 caretPosition = _document.GetCaretPosition(Position, _editorStyle);
        if (_caretPosition != caretPosition)
        {
            _caretPosition = MathHelper.LarpVector(_caretPosition, caretPosition, CARET_SPEED * (float)deltaTime);
        }
    }

    public void Render(Renderer renderer)
    {
        string[] lines = _document.Text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            Vector2 linePosition = Vector2.Round(Position + new Vector2(0, i * _lineSpacing));

            if (linePosition.Y < 0 || linePosition.Y > App.WindowHeight)
                break;

            renderer.RenderText(App.Font, _pointSize, lines[i], linePosition, Color.White);
        }

        // Caret
        {
            Rectangle caret = new(_caretPosition, 2, _pointSize);
            renderer.RenderFilledRectangle(caret, Color.RoyalBlue);
        }

        // Top bar
        {
            renderer.RenderFilledRectangle(new(0, 0, App.WindowWidth, TOP_BAR_HEIGHT), App.BackgroundColor);
            renderer.RenderLine(new(0, TOP_BAR_HEIGHT), new(App.WindowWidth, TOP_BAR_HEIGHT), App.VeryLightColor);
        }

        // Opened file text
        {
            Vector2 filePathTextSize = App.Font.MeasureString(_fileManager.DisplayPath, App.SMALL_POINT_SIZE);
            Vector2 filePathTextPosition = new(App.WindowWidth - filePathTextSize.X - App.DEFAULT_PADDING, (TOP_BAR_HEIGHT / 2) - (filePathTextSize.Y / 2.5f));
            renderer.RenderText(App.Font, App.SMALL_POINT_SIZE, _fileManager.DisplayPath, filePathTextPosition, Color.White);
        }
    }
}
