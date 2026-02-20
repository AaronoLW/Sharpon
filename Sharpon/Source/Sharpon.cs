using Color = System.Drawing.Color;
using SDL3;
using Smash;
using Smash.Graphics;
using System.Numerics;

public class Sharpon : Application
{
    public static Font Font = null!;
    public static readonly Vector2 EditorPosition = new Vector2(100, 150);

    private Renderer _renderer;
    private Window _window;

    private double _fps;
    private double _elapsedTime;

    private string? _textInput = null;

    private string _text = "";
    private int _charIndex = 0;
    private Vector2 _caretPosition;

    private string _testFileName = "sharpon.txt";

    public Sharpon()
    {
        CreateWindowAndRenderer("Sharpon", 800, 600, out _window, out _renderer);
        SDL.SetWindowResizable(_window.Handle, true);

        TTFInit();
        Font = LoadFont(Path.Combine("Fonts", "Rubik-Medium.ttf"), Settings.FONT_POINT_SIZE);

        Program.WindowResize += _window.OnWindowResized;
    }

    public override void Start()
    {
        SDL.StartTextInput(_window.Handle);

        _testFileName = Path.Combine(Directory.GetCurrentDirectory(), _testFileName);
        if (!File.Exists(_testFileName))
        {
            File.Create(_testFileName);
        }

        _text = LoadFile(_testFileName);
    }

    public override void Update(double deltaTime)
    {
        _elapsedTime += deltaTime;

        if (_elapsedTime > 0.5f)
        {
            _fps = 1 / deltaTime;
            _elapsedTime = 0;
        }

        KeybindHandlerInfo keybindHandlerInfo = KeybindHandler.HandleKeybinds(_text, _charIndex);

        _text = keybindHandlerInfo.NewText;
        _charIndex = keybindHandlerInfo.NewCharIndex;

        if (keybindHandlerInfo.EditorCommand != null)
        {
            if (keybindHandlerInfo.EditorCommand == EditorCommand.SaveFile)
            {
                SaveFile(_testFileName);
            }
        }

        if (_textInput != null && keybindHandlerInfo.MayAddTextInput)
        {
            _text += _textInput;
            _charIndex += _textInput.Length;

            _textInput = null;
        }

        Vector2 preferredCaretPosition = GetPreferredCaretPosition();
        _caretPosition = MathHelper.LerpVector(_caretPosition, preferredCaretPosition, Settings.CARET_SPEED * (float)deltaTime);
    }

    public override void Render()
    {
        _renderer.Clear(Settings.BACKGROUND_COLOR);

        string[] lines = BreakupTextToLines(_text);
        
        for (int i = 0; i < lines.Length; i++)
        {
            _renderer.RenderText(Font, lines[i], new Vector2(EditorPosition.X, EditorPosition.Y + (i * Settings.LINESPACING)), Color.White);
        }

        Rectangle caretRectangle = new Rectangle(_caretPosition, 3, Settings.FONT_POINT_SIZE);
        _renderer.RenderFilledRectangle(caretRectangle, Color.RoyalBlue);

        string fpsString = (int)_fps + " FPS";
        Vector2 FpsTextPosition = new Vector2(_window.Width - Font.MeasureString(fpsString).X - 20, 20);
        _renderer.RenderText(Font, fpsString, FpsTextPosition, Color.White);

        _renderer.RenderPresent();
    }

    public override void End()
    {
        SDL.StopTextInput(_window.Handle);
    }

    public void SendTextInput(string textInput)
    {
        _textInput = textInput;
    }

    private string[] BreakupTextToLines(string text)
    {
        return text.Split("\n");
    }

    private Vector2 GetPreferredCaretPosition()
    {
        string[] lines = BreakupTextToLines(_text);
        return EditorPosition + new Vector2(Font.MeasureString(lines[lines.Length - 1]).X, Settings.LINESPACING * (lines.Length - 1));
    }

    private string LoadFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            string fileContent = File.ReadAllText(filePath);
            return fileContent;
        }
        else
        {
            throw new FileNotFoundException($"File at {filePath} doesn't exist");
        }
    }

    private void SaveFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            File.WriteAllText(filePath, _text);
            Console.WriteLine("Saved File!");
        }
        else
        {
            throw new FileNotFoundException($"File at {filePath} doesn't exist");
        }
    }
}