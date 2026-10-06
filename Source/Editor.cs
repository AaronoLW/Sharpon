using System.Numerics;
using SDL3;
using SmashFramework;

using Color = System.Drawing.Color;

public class Editor : IPermutable, IRenderable, IActivityBarProvider
{
    private const float CARET_SPEED = 40f;

    private const int MIN_POINT_SIZE = 4;

    private TextDocument _document = null!;
    private PathManager _pathManager;
    private readonly Scroller _scroll = new(0, null);

    private IKeybindSet _keybindSet = new VimSet();

    private int _pointSize = App.POINT_SIZE;
    private float _lineSpacing => _pointSize + App.LINE_SPACING_POINT_SIZE_INCREASE;

    private Vector2 _caretPosition = new();

    private bool _unsavedChanges = false;

    public bool ShouldRender { get; set; } = true;

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
                _pointSize++;
            }

            if (Program.IsKeyDown(SDL.Keycode.Minus))
            {
                if (_pointSize > MIN_POINT_SIZE)
                    _pointSize--;
            }
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
        if (_document.CaretStyle == CaretStyle.Beam)
        {
            Rectangle caret = new(_caretPosition, 2, App.Font.MeasureString("|", _pointSize).Y);
            renderer.RenderFilledRectangle(caret, Color.RoyalBlue);
        }
        else
        {
            Rectangle caret = new(_caretPosition, App.Font.MeasureString("|", _pointSize).X, App.Font.MeasureString("|", _pointSize).Y);
            renderer.RenderFilledRectangle(caret, Color.RoyalBlue);
        }

        List<string> lines = _document.Lines;

        int startLineIndex = Math.Max((int)(_scroll.Scroll / _lineSpacing) - 1, 0);
        for (int i = startLineIndex; i < lines.Count; i++)
        {
            Vector2 linePosition = Vector2.Round(App.ActivityPosition + new Vector2(0, (i * _lineSpacing) - _scroll.Scroll));

            if (linePosition.Y < 0 || linePosition.Y > App.WindowHeight)
                break;

            renderer.RenderText(App.Font, _pointSize, lines[i], linePosition, Color.White);
        }
    }

    public Activity ProvideActivityBar()
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
