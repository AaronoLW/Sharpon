using Color = System.Drawing.Color;
using SmashFramework;
using System.Numerics;
using KatziDrip;
using Ghostflyby.Pty;
using XTerm.Buffer;
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

    private readonly Scroller _scroll = new();

    private readonly PtyProcess _pty;
    private readonly XTerm.Terminal _terminal;

    private readonly float _characterWidth;

    private Vector2 _previousWindowSize = App.WindowSize;

    public Terminal()
    {
        _characterWidth = FontManager.FontRegular.MeasureString("W", App.POINT_SIZE).X;

        _pty = PtyProcess.Start("bash", []);
        _pty.Input.AutoFlush = true;

        _terminal = new();

        _ = ReadPty();

        _inputField = new()
        {
            Prefix = null,
            Width = App.WindowWidth,
            Height = INPUT_FIELD_HEIGHT,
        };

        _inputField.OnEnter += InputFieldEnter;
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

        if (_previousWindowSize != App.WindowSize)
        {
            _previousWindowSize = App.WindowSize;
            _terminal.Resize((int)(App.WindowWidth / _characterWidth - 2), (int)(MAX_HEIGHT / _characterWidth - 2));
        }

        if ((_terminal.Buffer.Y + 2) * (App.POINT_SIZE + App.LINE_SPACING_POINT_SIZE_INCREASE) >= Height)
        {
            _scroll.SetScroll((_terminal.Buffer.Y + 2) * (App.POINT_SIZE + App.LINE_SPACING_POINT_SIZE_INCREASE) - MAX_HEIGHT);
        }
        else
        {
            _scroll.SetScroll(0);
        }

        _scroll.Update();
        _scroll.Animate(deltaTime);
    }

    public void Render(Renderer renderer)
    {
        Rectangle backgroundRectangle = new(Position, App.WindowWidth, App.TERMINAL_HEIGHT);
        renderer.RenderFilledRectangle(backgroundRectangle, Color.Black);
        renderer.RenderLine(0, Position.Y, App.WindowWidth, Position.Y, Drip.AccentColor.ToColor());

        Rectangle clipRect = new(Position + new Vector2(0, 1), App.WindowSize);
        SDL.SetRenderClipRect(renderer.Handle, clipRect.ToSDLRect());

        RenderTerminal(renderer);

        _inputField.RenderCaret(renderer);
        _inputField.Render(renderer);

        SDL.SetRenderClipRect(renderer.Handle, 0);
    }

    private void RenderTerminal(Renderer renderer)
    {
        var buffer = _terminal.Buffer;

        for (int row = 0; row < buffer.Rows; row++)
        {
            var line = buffer.Lines[buffer.YDisp + row];
            if (line == null)
                continue;

            for (int col = 0; col < buffer.Cols; col++)
            {
                BufferCell cell = line[col];

                if (cell.Width == 0)
                    continue;

                string character = cell.Content;

                Color color;
                if (cell.Attributes.Fg == 256)
                    color = Color.White;
                else
                    color = XtermEx.XtermColor(cell.Attributes.Fg);

                Vector2 position = TextPosition + new Vector2(_characterWidth * col, ((App.POINT_SIZE + App.LINE_SPACING_POINT_SIZE_INCREASE) * row) - _scroll.Scroll);
                renderer.RenderString(FontManager.FontRegular, App.POINT_SIZE, character, position, color);
            }
        }
    }

    public void TextEdit(IKeybindSet keybindSet)
    {
        if (Input.IsKeyDown(SDL.Keycode.Tab))
        {
            return;
        }

        _inputField.CaretStyle = keybindSet.GetCaretStyle();
        keybindSet.HandleKeybinds(_inputField);
    }

    private void InputFieldEnter()
    {
        _pty.Input.WriteLine(_inputField.Text);
        _inputField.Clear();
    }

    private Vector2 GetInputFieldPosition()
    {
        Console.WriteLine(_terminal.Buffer.Y);
        return TextPosition + new Vector2(((_terminal.Buffer.Lines[_terminal.Buffer.Y] ?? new(0)).GetTrimmedLength() + 1) * _characterWidth, _terminal.Buffer.Y * (App.POINT_SIZE + App.LINE_SPACING_POINT_SIZE_INCREASE) - _scroll.Scroll);
    }

    private async Task ReadPty()
    {
        while (true)
        {
            var stream = _pty.Output;

            char[] buffer = new char[1024];
            int bytesRead = await stream.ReadAsync(buffer);

            _terminal.Write(new string(buffer[0..bytesRead]));
        }
    }
}
