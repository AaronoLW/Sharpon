using System.Numerics;
using Smash;
using Smash.Graphics;
using Color = System.Drawing.Color;

public class App : Application 
{
    public const int BASE_POINT_SIZE = 20;

    private const int CARET_SPEED = 67;

    public static float ScaleFactor => (float)PointSize / (float)BASE_POINT_SIZE;

    public static int PointSize = BASE_POINT_SIZE;
    public static Font Font => AssetManager.GetFont("JetBrainsMono-Bold", PointSize);

    private Vector2 _editorStartPos = new Vector2(40);
    private Vector2 _caretPosition;

    private Window _window;
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
        }

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
            Vector2 textPosition = _editorStartPos + new Vector2(0, (PointSize + 7) * i);
            if (textPosition.Y > _window.Height) break;

            _renderer.RenderText(Font, lines[i], textPosition, Color.White);
        }

        _renderer.RenderFilledRectangle(new Rectangle(_caretPosition + new Vector2(-1, 3), 2 * ScaleFactor, PointSize), Color.RoyalBlue);

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
                float y = Font.MeasureString("|").Y * (lineIndex + 1);

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