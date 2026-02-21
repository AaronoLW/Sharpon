using Color = System.Drawing.Color;
using SDL3;
using Smash;
using Smash.Graphics;
using System.Numerics;
using System.Text.RegularExpressions;

public class Sharpon : Application
{
    private static Dictionary<float, Font> _fonts = new();
    public static readonly Vector2 EditorPosition = new Vector2(75, 100);

    public static float PointSize = Settings.FONT_POINT_SIZE;

    private static Sharpon _sharponReference = null!;

    private Renderer _renderer;
    private Window _window;

    private double _fps;
    private double _elapsedTime;

    private string? _textInput = null;

    private string _text = "";
    private int _charIndex = 0;
    private Vector2 _caretPosition;

    private string _filePath = "";

    private FileDialog _fileDialog;

    public Sharpon()
    {
        CreateWindowAndRenderer("Sharpon", 800, 600, out _window, out _renderer);
        SDL.SetWindowResizable(_window.Handle, true);

        TTFInit();

        Program.WindowResize += _window.OnWindowResized;

        _sharponReference = this;
        _fileDialog = new(_window);

        _renderer.SetRenderBlendMode(SDL.BlendMode.Blend);
    }

    public override void Start()
    {
        SDL.StartTextInput(_window.Handle);
    }

    public override void Update(double deltaTime)
    {
        _elapsedTime += deltaTime;

        if (_elapsedTime > 0.5f)
        {
            _fps = 1 / deltaTime;
            _elapsedTime = 0;
        }


        bool canReceiveTextInput = !_fileDialog.Opened;

        if (_fileDialog.Opened)
        {
            _fileDialog.SendTextInput(_textInput);
        }

        _fileDialog.Update(deltaTime);

        if (canReceiveTextInput)
        {
            KeybindHandlerInfo keybindHandlerInfo = KeybindHandler.HandleKeybinds(_text, _charIndex);

            _text = keybindHandlerInfo.NewText;
            _charIndex = keybindHandlerInfo.NewCharIndex;

            if (keybindHandlerInfo.EditorCommand != null)
            {
                if (keybindHandlerInfo.EditorCommand == EditorCommand.SaveFile)
                {
                    SaveFile(_filePath);
                }

                if (keybindHandlerInfo.EditorCommand == EditorCommand.ZoomIn)
                {
                    PointSize += 3;
                }

                if (keybindHandlerInfo.EditorCommand == EditorCommand.ZoomOut)
                {
                    int minPointSize = 12;
                
                    if (PointSize - 3 <= minPointSize)
                    {
                        PointSize = minPointSize;
                    }
                    else
                    {
                        PointSize -= 3;
                    }
                }

                if (keybindHandlerInfo.EditorCommand == EditorCommand.ToggleFileDialog)
                {
                    _fileDialog.Opened = true;
                }

                if (keybindHandlerInfo.EditorCommand == EditorCommand.ArrowUp)
                {
                    for (int i = _charIndex - 1; i > 0; i--)
                    {
                        if (_text[i] == '\n')
                        {
                            _charIndex = i;
                            break;
                        }
                    }
                }

                if (keybindHandlerInfo.EditorCommand == EditorCommand.ArrowDown)
                {
                    for (int i = _charIndex; i < _text.Length; i++)
                    {
                        if (_text[i] == '\n')
                        {
                            for (int j = i + 1; j < _text.Length; j++)
                            {
                                if (_text[j] == '\n')
                                {
                                    _charIndex = j;
                                    goto endDownArrowLoop;
                                } 
                            }
                        }

                        if (i + 1 == _text.Length)
                        {
                            _charIndex = _text.Length;
                            break;
                        }

                    }

                    endDownArrowLoop:
                        //Do nothing so the compiler doesnt throw an error or warning
                        keybindHandlerInfo.EditorCommand = EditorCommand.ArrowDown;
                }

                if (keybindHandlerInfo.EditorCommand == EditorCommand.Enter)
                {
                    _text = _text.Insert(_charIndex, "\n");
                    _charIndex++;
                }
            }

            if (_textInput != null && keybindHandlerInfo.MayAddTextInput)
            {
                _text = _text.Insert(_charIndex, _textInput);
                _charIndex += _textInput.Length;

            }
        }

        _textInput = null;

        Vector2 preferredCaretPosition = GetPreferredCaretPosition();
        _caretPosition = MathHelper.LerpVector(_caretPosition, preferredCaretPosition, Settings.CARET_SPEED * (float)deltaTime);
    }

    public override void Render()
    {
        _renderer.Clear(Settings.BACKGROUND_COLOR);

        Font font = GetOrCreateFont(PointSize);
        Font smallFont = GetOrCreateFont(PointSize - 8);

        string[] lines = BreakupTextToLines(_text);
        
        float lineSpacing = PointSize + 6;

        for (int i = 0; i < lines.Length; i++)
        {
            _renderer.RenderText(font, lines[i], new Vector2(EditorPosition.X, EditorPosition.Y + (i * lineSpacing)), Color.White);
        }

        bool editorFocused = !_fileDialog.Opened;

        if (editorFocused)
        {
            Rectangle caretRectangle = new Rectangle(_caretPosition, Settings.CARET_WIDTH, PointSize);
            _renderer.RenderFilledRectangle(caretRectangle, Color.RoyalBlue);
        }

        string fpsString = (int)_fps + " FPS";
        Vector2 fpsTextPosition = new Vector2(_window.Width - smallFont.MeasureString(fpsString).X - 20, 20);
        _renderer.RenderText(smallFont, fpsString, fpsTextPosition, Color.White);

        Vector2 filePathTextPosition = fpsTextPosition - new Vector2(smallFont.MeasureString(_filePath).X + 30, 0);
        _renderer.RenderText(smallFont, _filePath, filePathTextPosition, Color.White);

        _fileDialog.Render(_renderer);

        _renderer.RenderPresent();
    }

    public override void End()
    {
        foreach (Font font in _fonts.Values)
        {
            font.Free();
        }

        SDL.StopTextInput(_window.Handle);
    }

    public void SendTextInput(string textInput)
    {
        _textInput = textInput;
    }

    public static Font GetOrCreateFont(float pointSize)
    {
        if (_fonts.TryGetValue(pointSize, out Font? font))
        {
            return font!;
        }
        else
        {
            Font newFont = _sharponReference.LoadFont(Path.Combine("Fonts", "Rubik-Medium.ttf"), pointSize);
            _fonts.Add(pointSize, newFont);
            return _fonts[pointSize];
        }
    }

    private string[] BreakupTextToLines(string text, bool keepDelimiters = false)
    {
        if (keepDelimiters)
        {
            return Regex.Split(text, "(\n)");
        }
        else
        {
            return text.Split("\n");
        }
    }

    private Vector2 GetPreferredCaretPosition()
    {
        Font font = GetOrCreateFont(PointSize);

        int lineIndex = GetLineIndexFromCharIndex();
        int lineCharIndex = GetCharIndexOnLine();

        float lineSpacing = PointSize + 6;

        string[] lines = BreakupTextToLines(_text);
        return EditorPosition + new Vector2(font.MeasureString(lines[lineIndex].Substring(0, lineCharIndex)).X - Settings.CARET_WIDTH / 2, lineSpacing * lineIndex + 2);
    }

    private int GetCharIndexOnLine()
    {
        int index = 0;

        for (int i = _charIndex - 1; i > 0; i--)
        {
            if (_text[i] == '\n') return index;

            index++;
        }

        return _charIndex;
    }

    private int GetLineIndexFromCharIndex()
    {
        int lineAmount = 0;
        for (int i = _charIndex - 1; i > 0; i--)
        {
            if (_text[i] == '\n') lineAmount++;
        }

        if (lineAmount == 0) return 0;
        return lineAmount;
    }

    public static string LoadFile(string filePath, bool createIfMissing = false)
    {
        if (!File.Exists(filePath))
        {
            if (createIfMissing)
            {
                using (FileStream fileStream = File.Create(filePath)) {}
            }
            else
            {
                throw new FileNotFoundException($"File at {filePath} doesn't exist");
            }
        }

        string fileContent = File.ReadAllText(filePath);
        _sharponReference._filePath = filePath;
        _sharponReference._text = fileContent;
        _sharponReference._charIndex = fileContent.Length;

        return fileContent;
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