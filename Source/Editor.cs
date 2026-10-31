using Color = System.Drawing.Color;
using System.Numerics;
using KatziDrip;
using SDL3;
using SmashFramework;

public class Editor : IPermutable, IRenderable, IActivityProvider, IRequireTextEditing
{
    private const float CARET_SPEED = 40f;

    private TextDocument _document = null!;
    private int _previousLineIndex = 0;

    private PathManager _pathManager;
    private readonly Scroller _scroll = new(0, null);

    private Vector2 _caretPosition = new();

    private bool _unsavedChanges = false;

    public bool ShouldRender { get; set; } = true;

    public Editor(string? initialFilePath)
    {
        _pathManager = new("No file opened");
        OpenFile(initialFilePath);
    }

    public void TextEdit(IKeybindSet keybindSet)
    {
        if (keybindSet.HandleKeybinds(_document))
            _unsavedChanges = true;

        _document.CaretStyle = keybindSet.GetCaretStyle();
        _scroll.Update();

    }

    public void Update(double deltaTime)
    {
        if (Input.IsKeyDown(SDL.Keycode.LCtrl))
        {
            if (Program.IsKeyDown(SDL.Keycode.S))
            {
                if (_pathManager.SaveFile(_document))
                    _unsavedChanges = false;
            }
        }

        if (Input.MouseX < App.WindowWidth - App.FILEMANAGER_WIDTH)
        {
            if (Input.IsLeftMousePressed())
            {
                _document.JumpToMousePosition(App.ActivityPosition - new Vector2(0, _scroll.Scroll));
            }

            if (Input.IsKeyDown(SDL.Keycode.LCtrl))
            {
                if (Program.IsKeyDown(SDL.Keycode.Plus))
                {
                    PointSizeDeleter.Enqueue(_document.PointSize);
                    _document.PointSize++;
                }

                if (Program.IsKeyDown(SDL.Keycode.Minus))
                {
                    if (_document.PointSize > TextContainer.MIN_POINT_SIZE)
                    {
                        PointSizeDeleter.Enqueue(_document.PointSize);
                        _document.PointSize--;
                    }
                }
            }
        }

        _scroll.Clamp();
        _scroll.Animate(deltaTime);

        Vector2 documentOffset = App.ActivityPosition with { Y = App.ActivityPosition.Y - _scroll.Scroll };
        Vector2 caretPosition = _document.GetCaretPosition(documentOffset);
        if (_caretPosition != caretPosition)
        {
            _caretPosition = MathHelper.LarpVector(_caretPosition, caretPosition, CARET_SPEED * (float)deltaTime);
        }

        if (_previousLineIndex != _document.LineIndex)
        {
            _scroll.SetScroll((_document.LineIndex * _document.LineSpacing) - (App.WindowHeight / 2) + App.ACTIVITY_BAR_HEIGHT);
            _previousLineIndex = _document.LineIndex;
        }
    }

    public void Render(Renderer renderer)
    {
        RenderCaret(renderer, App.ActivityPosition - new Vector2(0, _scroll.Scroll));

        int startLineIndex = Math.Max((int)(_scroll.Scroll / _document.LineSpacing) - 1, 0);
        for (int i = startLineIndex; i < _document.Lines.Count; i++)
        {
            Vector2 linePosition = Vector2.Round(App.ActivityPosition + new Vector2(0, (i * _document.LineSpacing) - _scroll.Scroll));

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

    private void RenderCaret(Renderer renderer, Vector2 offset)
    {
        Vector2 caretPosition = _document.GetCaretPosition(offset);

        if (_document.CaretStyle == CaretStyle.Beam)
        {
            Rectangle caret = new(caretPosition, 2, FontManager.FontRegular.MeasureString("|", _document.PointSize).Y);
            renderer.RenderFilledRectangle(caret, Drip.AccentColor.ToColor());
        }
        else if (_document.CaretStyle == CaretStyle.Block)
        {
            Rectangle caret = new(caretPosition, FontManager.FontRegular.MeasureString("|", _document.PointSize).X, FontManager.FontRegular.MeasureString("|", _document.PointSize).Y);
            renderer.RenderFilledRectangle(caret, Drip.AccentColor.ToColor());
        }
        else if (_document.CaretStyle == CaretStyle.Underline)
        {
            Rectangle caret = new(caretPosition + new Vector2(0, _document.PointSize), FontManager.FontRegular.MeasureString("|", _document.PointSize).X, 4);
            renderer.RenderFilledRectangle(caret, Drip.AccentColor.ToColor());
        }
    }

    public Activity ProvideActivity()
    {
        return new(_pathManager.DisplayPath, _unsavedChanges ? "Unsaved Changes" : "");
    }

    public void OpenFile(string? filePath)
    {
        _document = _pathManager.OpenFile(filePath);
        _scroll.Reset();
        _previousLineIndex = 0;
        _unsavedChanges = false;
    }
}
