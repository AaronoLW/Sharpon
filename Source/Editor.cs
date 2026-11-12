using Color = System.Drawing.Color;
using System.Numerics;
using KatziDrip;
using SDL3;
using SmashFramework;

public class Editor : IPermutable, IRenderable, IActivityProvider, IRequireTextEditing
{
    private const int HORIZONTAL_EDITOR_OFFSET = 80;

    private TextDocument _document = null!;

    private PathManager _pathManager;
    private readonly Scroller _scroll = new(0, null);

    private bool _unsavedChanges = false;

    public Vector2 TextPosition => App.ActivityPosition - new Vector2(-HORIZONTAL_EDITOR_OFFSET, _scroll.Scroll);
    public Vector2 CaretPosition => _document.GetCaretPosition(TextPosition);

    public bool ShouldRender { get; set; } = true;

    private float _minCaretPosition => App.ACTIVITY_BAR_HEIGHT + FontManager.FontRegular.MeasureString("|", _document.PointSize).Y * 5;
    private float _maxCaretPosition => App.WindowHeight - FontManager.FontRegular.MeasureString("|", _document.PointSize).Y * 7;

    private int _previousLineIndex = 0;

    private bool _leftMouseDownForSelecting = false;
    private (int, int)? _lastMouseDocumentPosition;

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

        if (Input.MouseX < App.WindowWidth - App.FileManagerWidth && App.MainWindowHasFocus && Input.MouseY < App.WindowHeight - App.TerminalHeight)
        {
            if (Input.IsLeftMouseDown())
            {
                _document.JumpToMousePosition(App.ActivityPosition - new Vector2(-HORIZONTAL_EDITOR_OFFSET, _scroll.Scroll));

                if (!_leftMouseDownForSelecting)
                {
                    if (_document.SelectionMode != SelectionMode.None)
                        _document.EndSelection();

                    if (_document.GetPositionAtMouse(App.ActivityPosition - new Vector2(-HORIZONTAL_EDITOR_OFFSET, _scroll.Scroll)) != _lastMouseDocumentPosition)
                    {
                        _leftMouseDownForSelecting = true;
                        _document.StartSelection(SelectionMode.Default, _lastMouseDocumentPosition);
                    }
                }
            }
            else
            {
                _leftMouseDownForSelecting = false;
            }

            _lastMouseDocumentPosition = _document.GetPositionAtMouse(App.ActivityPosition - new Vector2(-HORIZONTAL_EDITOR_OFFSET, _scroll.Scroll));

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

            _scroll.Update();
        }

        _scroll.Clamp();
        _scroll.Animate(deltaTime);

        if (_previousLineIndex != _document.LineIndex)
        {
            if (CaretPosition.Y < _minCaretPosition)
            {
                _scroll.SetScroll(_scroll.Scroll + (CaretPosition.Y - _minCaretPosition));
            }

            if (CaretPosition.Y > _maxCaretPosition)
            {
                _scroll.SetScroll(_scroll.Scroll + (CaretPosition.Y - _maxCaretPosition));
            }

            _previousLineIndex = _document.LineIndex;
        }
    }

    public void Render(Renderer renderer)
    {
        RenderCaret(renderer, TextPosition);

        // Separator line between the line numbers and text
        renderer.RenderLine(HORIZONTAL_EDITOR_OFFSET, 0, HORIZONTAL_EDITOR_OFFSET, App.WindowHeight, Drip.VeryLightColor.ToColor());

        int startLineIndex = Math.Max((int)(_scroll.Scroll / _document.LineSpacing) - 1, 0);
        for (int i = startLineIndex; i < _document.Lines.Count; i++)
        {
            // Line numbers
            {
                Vector2 lineNumberPosition = Vector2.Round(App.ActivityPosition + new Vector2(0, (i * _document.LineSpacing) - _scroll.Scroll));

                Color color = _document.LineIndex == i ? Drip.PrimaryAccentColor.ToColor() : Color.White;
                renderer.RenderString(FontManager.FontRegular, _document.PointSize, (i + 1).ToString(), lineNumberPosition, color);
            }

            Vector2 linePosition = Vector2.Round(App.ActivityPosition + new Vector2(HORIZONTAL_EDITOR_OFFSET, (i * _document.LineSpacing) - _scroll.Scroll));

            if (linePosition.Y < 0 || linePosition.Y > App.WindowHeight)
                break;

            (int, int)? selectionRange = _document.GetSelectionRangeOnLine(i);
            if (selectionRange != null)
            {
                float fromWidth = FontManager.FontRegular.MeasureString(_document.Lines[i][0..selectionRange.Value.Item1], _document.PointSize).X;
                float toWidth = FontManager.FontRegular.MeasureString(_document.Lines[i][0..selectionRange.Value.Item2], _document.PointSize).X - fromWidth;

                // If there are no characters on the line set a minimum
                if (toWidth == 0)
                    toWidth = 5;

                Rectangle selectionRect = new(linePosition + new Vector2(fromWidth, 0), new Vector2(toWidth, FontManager.FontRegular.MeasureString("|", _document.PointSize).Y));

                Kolor accentColor = new(Drip.PrimaryAccentColor.R, Drip.PrimaryAccentColor.G, Drip.PrimaryAccentColor.B, 100);
                renderer.RenderFilledRectangle(selectionRect, accentColor.ToColor());
            }

            renderer.RenderString(FontManager.FontRegular, _document.PointSize, _document.Lines[i], linePosition, Color.White);
        }
    }

    private void RenderCaret(Renderer renderer, Vector2 offset)
    {
        Vector2 caretPosition = CaretPosition;

        if (_document.CaretStyle == CaretStyle.Beam)
        {
            Rectangle caret = new(caretPosition, 2, FontManager.FontRegular.MeasureString("|", _document.PointSize).Y);
            renderer.RenderFilledRectangle(caret, Drip.PrimaryAccentColor.ToColor());
        }
        else if (_document.CaretStyle == CaretStyle.Block)
        {
            Rectangle caret = new(caretPosition, FontManager.FontRegular.MeasureString("|", _document.PointSize).X, FontManager.FontRegular.MeasureString("|", _document.PointSize).Y);
            renderer.RenderFilledRectangle(caret, Drip.PrimaryAccentColor.ToColor());
        }
        else if (_document.CaretStyle == CaretStyle.Underline)
        {
            Rectangle caret = new(caretPosition + new Vector2(0, _document.PointSize), FontManager.FontRegular.MeasureString("|", _document.PointSize).X, 4);
            renderer.RenderFilledRectangle(caret, Drip.PrimaryAccentColor.ToColor());
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
        _unsavedChanges = false;
    }
}
