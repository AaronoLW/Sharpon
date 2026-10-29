using Color = System.Drawing.Color;
using SmashFramework;
using System.Numerics;
using KatziDrip;
using Ghostflyby.Pty;
using XTerm.Buffer;
using SDL3;

public class Terminal : IPermutable, IRenderable, IRequireTextEditing, IDisposable
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

    private readonly float _pointSize = 17;

    public Terminal()
    {
        _characterWidth = FontManager.FontRegular.MeasureString("W", _pointSize).X;

        string shellPath;

        if (OperatingSystem.IsWindows())
        {
            shellPath = "cmd.exe";
        }
        else
        {
            shellPath = Environment.GetEnvironmentVariable("SHELL") ?? "/bin/bash";
        }

        PtyStartInfo ptyInfo = new(shellPath)
        {
            Environment = new Dictionary<string, string?> { ["TERM"] = "xterm-256color" }
        };

        _pty = PtyProcess.Start(ptyInfo);
        _pty.Input.WriteLine("HIIIIII");
        _pty.Input.WriteLine("clear");

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

        if ((_terminal.Buffer.Y + 2) * (_pointSize + App.LINE_SPACING_POINT_SIZE_INCREASE) >= Height)
        {
            _scroll.SetScroll((_terminal.Buffer.Y + 2) * (_pointSize + App.LINE_SPACING_POINT_SIZE_INCREASE) - MAX_HEIGHT);
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
        Vector2 caretPosition = TextPosition + new Vector2(_terminal.Buffer.X * _characterWidth, _terminal.Buffer.Y * (_pointSize + App.LINE_SPACING_POINT_SIZE_INCREASE));
        CaretStyle caretStyle = XTermEx.ToCaretStyle(_terminal.Options.CursorStyle);

        if (caretStyle == CaretStyle.Beam)
        {
            Rectangle caret = new(caretPosition, 2, FontManager.FontRegular.MeasureString("|", _pointSize).Y);
            renderer.RenderFilledRectangle(caret, Drip.AccentColor.ToColor());
        }
        else if (caretStyle == CaretStyle.Block)
        {
            Rectangle caret = new(caretPosition, FontManager.FontRegular.MeasureString("|", _pointSize).X, FontManager.FontRegular.MeasureString("|", _pointSize).Y);
            renderer.RenderFilledRectangle(caret, Drip.AccentColor.ToColor());
        }
        else if (caretStyle == CaretStyle.Underline)
        {
            Rectangle caret = new(caretPosition + new Vector2(0, _pointSize), FontManager.FontRegular.MeasureString("|", _pointSize).X, 4);
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

                Vector2 position = TextPosition + new Vector2(_characterWidth * col, ((_pointSize + App.LINE_SPACING_POINT_SIZE_INCREASE) * row) - _scroll.Scroll);
                renderer.RenderString(FontManager.FontRegular, _pointSize, character, position, color);
            }
        }
    }

    public void TextEdit(IKeybindSet keybindSet)
    {
        _pty.Input.Write(Program.TextInput);

        if (!Input.IsKeyDown(SDL.Keycode.LCtrl))
        {
            if (Program.IsKeyDown(SDL.Keycode.Return))
                _pty.Input.Write("\n");

            if (Program.IsKeyDown(SDL.Keycode.Tab))
                _pty.Input.Write("\t");

            if (Program.IsKeyDown(SDL.Keycode.Backspace))
                _pty.Input.Write("\b");
        }
        else
        {
            if (Program.IsKeyDown(SDL.Keycode.Backspace))
                _pty.Input.Write("\x1b\x7f");

            if (Program.IsKeyDown(SDL.Keycode.C))
                _pty.Input.Write("\x3");

            if (Input.IsKeyDown(SDL.Keycode.LShift))
            {
                if (Program.IsKeyDown(SDL.Keycode.V))
                {
                    _pty.Input.Write(SDL.GetClipboardText());
                }
            }
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

    public void Dispose()
    {
        _terminal.Dispose();
        _pty.Dispose();
        _pty.Dispose();
    }
}
