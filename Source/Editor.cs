using System.Numerics;
using KatziDrip;
using SDL3;
using SmashFramework;

using Color = System.Drawing.Color;

public class Editor : IPermutable, IRenderable, IActivityProvider
{
    private const float CARET_SPEED = 40f;

    private const int MIN_POINT_SIZE = 4;

    private TextDocument _document = null!;
    private PathManager _pathManager;
    private readonly Scroller _scroll = new(0, null);

    private readonly IKeybindSet _keybindSet = new VimSet();

    private int _pointSize = App.POINT_SIZE;
    private float _lineSpacing => _pointSize + App.LINE_SPACING_POINT_SIZE_INCREASE;

    private Vector2 _caretPosition = new();

    private bool _unsavedChanges = false;

    public bool ShouldRender { get; set; } = true;

    private Queue<float> _scheduledPointSizeDeletions = new();

    public Editor(string? initialFilePath)
    {
        _pathManager = new("No file opened");
        OpenFile(initialFilePath);
    }

    public void Update(double deltaTime)
    {
        _keybindSet.HandleKeybinds(_document);

        if (Input.IsKeyDown(SDL.Keycode.LCtrl))
        {
            if (Program.IsKeyDown(SDL.Keycode.S))
            {
                if (_pathManager.SaveFile(_document))
                    _unsavedChanges = false;
            }

            if (Program.IsKeyDown(SDL.Keycode.Plus))
            {
                _scheduledPointSizeDeletions.Enqueue(_pointSize);
                _pointSize++;
            }

            if (Program.IsKeyDown(SDL.Keycode.Minus))
            {
                if (_pointSize > MIN_POINT_SIZE)
                {
                    _scheduledPointSizeDeletions.Enqueue(_pointSize);
                    _pointSize--;
                }
            }
        }

        if (Input.IsLeftMousePressed())
        {
            _document.JumpToMousePosition(App.ActivityPosition - new Vector2(0, _scroll.Scroll), _pointSize, _lineSpacing);
        }

        if (Input.MouseX < App.WindowWidth - App.FILEMANAGER_WIDTH)
            _scroll.Update();

        _scroll.Animate(deltaTime);

        Vector2 documentOffset = App.ActivityPosition with { Y = App.ActivityPosition.Y - _scroll.Scroll };
        Vector2 caretPosition = _document.GetCaretPosition(documentOffset, _pointSize, _lineSpacing);
        if (_caretPosition != caretPosition)
        {
            _caretPosition = MathHelper.LarpVector(_caretPosition, caretPosition, CARET_SPEED * (float)deltaTime);
        }
    }

    public void Render(Renderer renderer)
    {
        RenderCaret(renderer);

        List<string> lines = _document.Lines;

        int startLineIndex = Math.Max((int)(_scroll.Scroll / _lineSpacing) - 1, 0);
        for (int i = startLineIndex; i < lines.Count; i++)
        {
            Vector2 linePosition = Vector2.Round(App.ActivityPosition + new Vector2(0, (i * _lineSpacing) - _scroll.Scroll));

            if (linePosition.Y < 0 || linePosition.Y > App.WindowHeight)
                break;

            (int, int)? selectionRange = _document.GetSelectionRangeOnLine(i);
            if (selectionRange != null)
            {
                float fromWidth = App.Font.MeasureString(lines[i][0..selectionRange.Value.Item1], _pointSize).X;
                float toWidth = App.Font.MeasureString(lines[i][0..selectionRange.Value.Item2], _pointSize).X - fromWidth;

                Rectangle selectionRect = new(linePosition + new Vector2(fromWidth, 0), new Vector2(toWidth, App.Font.MeasureString(lines[i], _pointSize).Y));

                Kolor royalBlue = new(Drip.RoyalBlue.R, Drip.RoyalBlue.G, Drip.RoyalBlue.B, 100);
                renderer.RenderFilledRectangle(selectionRect, royalBlue.ToColor());
            }

            renderer.RenderString(App.Font, _pointSize, lines[i], linePosition, Color.White);
        }
    }

    public void AfterRender()
    {
        while (_scheduledPointSizeDeletions.Count > 0)
        {
            float pointSize = _scheduledPointSizeDeletions.Dequeue();
            AssetManager.Get<Font>(Drip.FontName).DeleteSizeIfExists(pointSize);
        }
    }

    private void RenderCaret(Renderer renderer)
    {
        if (_document.CaretStyle == CaretStyle.Beam)
        {
            Rectangle caret = new(_caretPosition, 2, App.Font.MeasureString("|", _pointSize).Y);
            renderer.RenderFilledRectangle(caret, Color.RoyalBlue);
        }
        else if (_document.CaretStyle == CaretStyle.Block)
        {
            Rectangle caret = new(_caretPosition, App.Font.MeasureString("|", _pointSize).X, App.Font.MeasureString("|", _pointSize).Y);
            renderer.RenderFilledRectangle(caret, Color.RoyalBlue);
        }
        else if (_document.CaretStyle == CaretStyle.Underline)
        {
            Rectangle caret = new(_caretPosition + new Vector2(0, _pointSize), App.Font.MeasureString("|", _pointSize).X, 4);
            renderer.RenderFilledRectangle(caret, Color.RoyalBlue);
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
