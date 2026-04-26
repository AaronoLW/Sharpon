using System.Numerics;
using Smash;
using Smash.Graphics;
using Color = System.Drawing.Color;

public class App : Application 
{
    public const int BASE_POINT_SIZE = 20;
    public const int CARET_SPEED = 67;

    private const int LINE_SPACING = 6;

    public static float ScaleFactor => (float)PointSize / (float)BASE_POINT_SIZE;

    public static int PointSize = BASE_POINT_SIZE;
    public static Font Font => AssetManager.GetFont("JetBrainsMono-Bold", PointSize);

    public static float WindowWidth => _window.Width;
    public static float WindowHeight =>  _window.Height;

    private Vector2 _editorStartPos = new Vector2(60, 40);
    private Vector2 _caretPosition = new(1);

    private static Window _window = null!;
    private Renderer _renderer;

    private string _text = "";
    private int _charIndex = 0;

    private float _elapsedTime;
    private double _fps;

    public App() 
    {
        CreateWindowAndRenderer("Sharpon", 800, 600, out _window, out _renderer);
        _window.SetWindowResizable(true);

        AssetManager.SetAssetRootDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets"));
        AssetManager.LoadFont(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "Fonts", "JetBrainsMono-Bold.ttf"));

        _renderer.SetVSyncEnabled(false);
        //_text = File.ReadAllText(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/sharpon_test.txt");
    }

    public override void Update(double deltaTime) 
    {
        _elapsedTime += (float)deltaTime;
        if (_elapsedTime > 0.5f)
        {
            _fps = 1f / deltaTime;
            _elapsedTime = 0;
        }

        TextHandler.Update(deltaTime);

        if (!FileDialog.Opened)
        {
            TextInputInfo textInfo = TextHandler.Handle(_text, _charIndex);
            _text = textInfo.NewText;
            _charIndex = textInfo.NewCharIndex;

            if (textInfo.TextInputOperation != null)
            {
                if (textInfo.TextInputOperation == TextInputOperation.IncreasePointSize)
                {
                    PointSize += 2;
                }

                if (textInfo.TextInputOperation == TextInputOperation.DecreasePointSize)
                {
                    PointSize -= 2;
                    PointSize = Math.Max(PointSize, 6);
                }

                if (textInfo.TextInputOperation == TextInputOperation.ToggleFileDialog)
                {
                    FileDialog.Open();
                }

                if (textInfo.TextInputOperation == TextInputOperation.DeleteWord)
                {
                    int jumpCharAmount = TextOperation.JumpLeft(_text, _charIndex);
                    _text = _text.Remove(_charIndex - jumpCharAmount, jumpCharAmount);
                    _charIndex -= jumpCharAmount;
                }

                if (textInfo.TextInputOperation == TextInputOperation.JumpLeft)
                {
                    int jumpCharAmount = TextOperation.JumpLeft(_text, _charIndex);
                    _charIndex -= jumpCharAmount;
                }

                if (textInfo.TextInputOperation == TextInputOperation.JumpRight)
                {
                    int jumpCharAmount = TextOperation.JumpRight(_text, _charIndex);
                    _charIndex += jumpCharAmount;
                }

                if (textInfo.TextInputOperation == TextInputOperation.DeleteCharacter)
                {
                    int deleteCharAmount = TextOperation.DeleteCharacter(_text, _charIndex);
                    if (deleteCharAmount > 1) _charIndex++;
                    _text = _text.Remove(_charIndex - deleteCharAmount, deleteCharAmount);
                    _charIndex -= deleteCharAmount;
                }

                if (textInfo.TextInputOperation == TextInputOperation.NewLine)
                {
                    (_text, _charIndex) = TextOperation.NewLine(_text, _charIndex);
                }

                if (textInfo.TextInputOperation == TextInputOperation.Tab)
                {
                    _text = _text.Insert(_charIndex, "    ");
                    _charIndex += 4;
                }

                if (textInfo.TextInputOperation == TextInputOperation.JumpDown)
                {
                    _charIndex = TextOperation.JumpDown(_text, _charIndex);
                }

                if (textInfo.TextInputOperation == TextInputOperation.JumpUp)
                {
                    _charIndex = TextOperation.JumpUp(_text, _charIndex);
                }

                if (textInfo.TextInputOperation == TextInputOperation.MoveLeft)
                {
                    if (_charIndex > 0)
                        _charIndex--;
                }

                if (textInfo.TextInputOperation == TextInputOperation.MoveRight)
                {
                    if (_charIndex + 1 <= _text.Length)
                        _charIndex++;
                }
            }
        }

        FileDialog.Update(deltaTime);

        Vector2 preferredCaretPosition = GetCaretPosition();
        if (_caretPosition != preferredCaretPosition)
        {
            _caretPosition = MathHelper.LerpVector(_caretPosition, preferredCaretPosition + _editorStartPos, CARET_SPEED * (float)deltaTime);
        }
    }

    public override void Render() 
    {
        _renderer.Clear(Color.FromArgb(25, 25, 28));

        string[] lines = _text.Split("\n");
        for (int i = 0; i < lines.Length; i++)
        {
            Vector2 textPosition = _editorStartPos + new Vector2(0, (PointSize + LINE_SPACING) * i);
            if (textPosition.Y > _window.Height) break;

            _renderer.RenderText(Font, lines[i], textPosition, Color.White);
            _renderer.RenderText(Font, i.ToString(), new Vector2(50 - Font.MeasureString(i.ToString()).X, textPosition.Y), Color.Gray);
        }

        _renderer.RenderFilledRectangle(new Rectangle(_caretPosition + new Vector2(-1, 3), 2 * ScaleFactor, PointSize), Color.RoyalBlue);

        FileDialog.Render(_renderer);

        //string fpsText = $"Fps: {(int)_fps}";
        //_renderer.RenderText(Font, fpsText, new Vector2(_window.Width - Font.MeasureString(fpsText).X - 20, 20), Color.White);

        _renderer.RenderPresent();
    }

    private Vector2 GetCaretPosition()
    {
        string[] lines = _text.Split("\n");
        int charAmount = 0;
        int lineIndex = -1;

        for (int i = 0; i < lines.Length; i++)
        {
            int lineLength = lines[i].Length;

            if (charAmount + lineLength >= _charIndex)
            {
                int charIndexInLine = _charIndex - charAmount;

                string textBeforeCaret = lines[i].Substring(0, charIndexInLine);
                float x = Font.MeasureString(textBeforeCaret).X;
                float y = (PointSize + LINE_SPACING) * (lineIndex + 1);

                return new Vector2(x, y);
            }

            charAmount += lineLength + 1;
            lineIndex++;
        }

        return Vector2.Zero;
    }

    public override void End() 
    {
        _window.Dispose();
        _renderer.Dispose();
        AssetManager.Dispose();
    }
}