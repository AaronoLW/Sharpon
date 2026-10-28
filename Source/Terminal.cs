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

        RenderCaret(renderer);
        RenderTerminal(renderer);

        SDL.SetRenderClipRect(renderer.Handle, 0);
    }

    private void RenderCaret(Renderer renderer)
    {
        Vector2 caretPosition = TextPosition + new Vector2(_terminal.Buffer.X * _characterWidth, _terminal.Buffer.Y * (App.POINT_SIZE + App.LINE_SPACING_POINT_SIZE_INCREASE));
        CaretStyle caretStyle = XTermEx.ToCaretStyle(_terminal.Options.CursorStyle);

        if (caretStyle == CaretStyle.Beam)
        {
            Rectangle caret = new(caretPosition, 2, FontManager.FontRegular.MeasureString("|", App.POINT_SIZE).Y);
            renderer.RenderFilledRectangle(caret, Drip.AccentColor.ToColor());
        }
        else if (caretStyle == CaretStyle.Block)
        {
            Rectangle caret = new(caretPosition, FontManager.FontRegular.MeasureString("|", App.POINT_SIZE).X, FontManager.FontRegular.MeasureString("|", App.POINT_SIZE).Y);
            renderer.RenderFilledRectangle(caret, Drip.AccentColor.ToColor());
        }
        else if (caretStyle == CaretStyle.Underline)
        {
            Rectangle caret = new(caretPosition + new Vector2(0, App.POINT_SIZE), FontManager.FontRegular.MeasureString("|", App.POINT_SIZE).X, 4);
            renderer.RenderFilledRectangle(caret, Drip.AccentColor.ToColor());
        }
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
                    color = XTermEx.XtermColor(cell.Attributes.Fg);

                Vector2 position = TextPosition + new Vector2(_characterWidth * col, ((App.POINT_SIZE + App.LINE_SPACING_POINT_SIZE_INCREASE) * row) - _scroll.Scroll);
                renderer.RenderString(FontManager.FontRegular, App.POINT_SIZE, character, position, color);
            }
        }
    }

    public void TextEdit(IKeybindSet keybindSet)
    {
        _pty.Input.Write(Program.TextInput);

        if (Program.IsKeyDown(SDL.Keycode.Return))
            _pty.Input.Write("\n");

        if (Program.IsKeyDown(SDL.Keycode.Tab))
            _pty.Input.Write("\t");

        if (Program.IsKeyDown(SDL.Keycode.Backspace))
        {
            if (Input.IsKeyDown(SDL.Keycode.LCtrl))
                _pty.Input.Write("\x1b\x7f");
            else
                _pty.Input.Write("\b");
        }
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
