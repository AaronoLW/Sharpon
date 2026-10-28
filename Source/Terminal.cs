using Color = System.Drawing.Color;
using SmashFramework;
using System.Numerics;
using KatziDrip;
using Ghostflyby.Pty;
using System.Text;
using SDL3;

public class Terminal : IPermutable, IRenderable, IRequireTextEditing
{
    public bool Opened;

    private bool _shouldRender = false;
    public bool ShouldRender { get => _shouldRender || Height >= 1; set => _shouldRender = value; }

    public const int MAX_HEIGHT = 350;

    public float Height { get; private set; } = 0;

    public Vector2 Position => new(0, App.WindowHeight - Height);
    public Vector2 TextPosition => Position + new Vector2(App.DEFAULT_PADDING);
    public Vector2 InputFieldPosition => GetInputFieldPosition();

    public const int INPUT_FIELD_HEIGHT = 60;
    private readonly InputField _inputField;

    private readonly TextDocument _document;
    private readonly Scroller _scroll = new();

    private readonly PtyProcess _pty;

    public Terminal()
    {
        _pty = PtyProcess.Start("bash", ["--noprofile", "-norc", "-i"]);
        _pty.Input.AutoFlush = true;

        _ = ReadPty();

        _inputField = new()
        {
            Prefix = null,
            Width = App.WindowWidth,
            Height = INPUT_FIELD_HEIGHT,
        };

        _inputField.OnEnter += InputFieldEnter;

        _document = new(null);
    }

    public void Update(double deltaTime)
    {
        if (Opened)
        {
            Height = MathHelper.Larp(Height, App.TERMINAL_HEIGHT, App.ANIMATION_SPEED * (float)deltaTime);
        }
        else
        {
            Height = MathHelper.Larp(Height, 0, App.ANIMATION_SPEED * (float)deltaTime);
        }

        _inputField.Position = InputFieldPosition;
    }

    public void Render(Renderer renderer)
    {
        Rectangle backgroundRectangle = new(Position, App.WindowWidth, App.TERMINAL_HEIGHT);
        renderer.RenderFilledRectangle(backgroundRectangle, Color.Black);
        renderer.RenderLine(0, Position.Y, App.WindowWidth, Position.Y, Drip.AccentColor.ToColor());

        RenderDocument(renderer);

        _inputField.RenderCaret(renderer);
        _inputField.Render(renderer);
    }

    private void RenderDocument(Renderer renderer)
    {
        int startLineIndex = Math.Max((int)(_scroll.Scroll / _document.LineSpacing) - 1, 0);
        for (int i = startLineIndex; i < _document.Lines.Count; i++)
        {
            Vector2 linePosition = Vector2.Round(TextPosition + new Vector2(0, (i * _document.LineSpacing) - _scroll.Scroll));

            if (linePosition.Y < 0 || linePosition.Y > App.WindowHeight)
                break;

            (int, int)? selectionRange = _document.GetSelectionRangeOnLine(i);
            if (selectionRange != null)
            {
                float fromWidth = FontManager.FontRegular.MeasureString(_document.Lines[i][0..selectionRange.Value.Item1], _document.PointSize).X;
                float toWidth = FontManager.FontRegular.MeasureString(_document.Lines[i][0..selectionRange.Value.Item2], _document.PointSize).X - fromWidth;

                Rectangle selectionRect = new(linePosition + new Vector2(fromWidth, 0), new Vector2(toWidth, FontManager.FontRegular.MeasureString(_document.Lines[i], _document.PointSize).Y));

                Kolor accentColor = new(Drip.AccentColor.R, Drip.AccentColor.G, Drip.AccentColor.B, 100);
                renderer.RenderFilledRectangle(selectionRect, accentColor.ToColor());
            }

            renderer.RenderString(FontManager.FontRegular, _document.PointSize, _document.Lines[i], linePosition, Color.White);
        }
    }

    public void TextEdit(IKeybindSet keybindSet)
    {
        _inputField.CaretStyle = keybindSet.GetCaretStyle();
        keybindSet.HandleKeybinds(_inputField);
    }

    private void InputFieldEnter()
    {
        _document.Insert(_inputField.Text);
        _pty.Input.WriteLine(_inputField.Text);
        _inputField.Clear();
    }

    private Vector2 GetInputFieldPosition()
    {
        Vector2 textSize = FontManager.FontRegular.MeasureString(_document.Lines[_document.LineIndex], _document.PointSize);
        return TextPosition + new Vector2(textSize.X, _document.LineSpacing * _document.LineIndex);
    }

    private async Task ReadPty()
    {
        while (true)
        {
            var stream = _pty.Output;

            char[] buffer = new char[1024];
            int bytesRead = await stream.ReadAsync(buffer);

            _document.Append(new string(buffer[0..bytesRead]));
            _document.JumpToEndOfFile();
        }
    }
}
