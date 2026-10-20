using System.Numerics;
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
            _scroll.Update();

            if (Input.IsLeftMousePressed())
            {
                _document.JumpToMousePosition(App.ActivityPosition - new Vector2(0, _scroll.Scroll));
            }
        }

        _scroll.Animate(deltaTime);

        Vector2 documentOffset = App.ActivityPosition with { Y = App.ActivityPosition.Y - _scroll.Scroll };
        Vector2 caretPosition = _document.GetCaretPosition(documentOffset);
        if (_caretPosition != caretPosition)
        {
            _caretPosition = MathHelper.LarpVector(_caretPosition, caretPosition, CARET_SPEED * (float)deltaTime);
        }

        if (_previousLineIndex != _document.LineIndex)
        {
            _scroll.SetScroll((_document.LineIndex * TextDocument.LineSpacing) - (App.WindowHeight / 2) + App.ACTIVITY_BAR_HEIGHT);
            _previousLineIndex = _document.LineIndex;
        }
    }

    public void Render(Renderer renderer)
    {
        Rectangle bounds = new(App.ActivityPosition, App.WindowWidth - App.FILEMANAGER_WIDTH, App.WindowHeight - App.ACTIVITY_BAR_HEIGHT);
        _document.Render(renderer, App.ActivityPosition, bounds, _scroll.Scroll);
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
