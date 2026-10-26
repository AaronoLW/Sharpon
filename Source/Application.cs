using System.Collections.ObjectModel;
using System.Numerics;
using SDL3;
using SmashFramework;
using KatziDrip;

public class App
{
    public static readonly string DATA_DIRECTORY_PATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Sharpon");

    public const int POINT_SIZE = 21;
    public const int SMALL_POINT_SIZE = 18;
    public const int VERY_SMALL_POINT_SIZE = 16;

    public const int LINE_SPACING_POINT_SIZE_INCREASE = 6;

    public const int DEFAULT_PADDING = 20;

    private const int INITIAL_WINDOW_WIDTH = 800;
    private const int INITIAL_WINDOW_HEIGHT = 600;

    public const int FILEMANAGER_WIDTH = 250;
    public const int ACTIVITY_BAR_HEIGHT = 50;
    public const int TERMINAL_HEIGHT = 350;

    public const int ANIMATION_SPEED = 30;

    public const int SCROLL_SPEED_AMPLIFIER = 40;

    public static readonly bool CSharpBracketStyle = true;

    public static int WindowWidth = INITIAL_WINDOW_WIDTH;
    public static int WindowHeight = INITIAL_WINDOW_HEIGHT;
    public static Vector2 WindowSize => new(WindowWidth, WindowHeight);

    public static readonly Vector2 ActivityPosition = new(App.DEFAULT_PADDING, ACTIVITY_BAR_HEIGHT + DEFAULT_PADDING);

    private static Window _window = null!;
    private static Renderer _renderer = null!;

    private static bool _windowFullscreen = false;

    private readonly Editor _editor;
    private readonly FileManager _fileManager;
    private readonly Config _config;
    private readonly Terminal _terminal;

    private readonly RichPresence _richPresence;

    private readonly ReadOnlyCollection<IPermutable> _permutables;
    private readonly List<IRenderable> _renderables;
    private readonly ReadOnlyCollection<IDisposable> _disposables;

    private readonly ActivityBar _activityBar = new();
    private IActivityProvider _activityProvider;

    private readonly Queue<float> _scheduledPointSizeDeletions = new();

    private IRequireTextEditing? _focusedTextEditor;

    private readonly IKeybindSet _keybindSet = new VimSet();

    public App(string? initialFilePath)
    {
        _window = Window.Create("Sharpon!", INITIAL_WINDOW_WIDTH, INITIAL_WINDOW_HEIGHT, SDL.WindowFlags.Resizable);
        _renderer = new(_window);
        _renderer.SetVSyncEnabled(true);

        _window.StartPollingTextInput();

        if (!Directory.Exists(DATA_DIRECTORY_PATH))
        {
            Directory.CreateDirectory(DATA_DIRECTORY_PATH);
        }

        Drip.Load(Drip.DefaultThemeName);
        FontManager.Init(_renderer);

        _renderer.SetVSyncEnabled(true);
        _renderer.SetRenderBlendMode(BlendMode.Blend);

        _editor = new(initialFilePath);
        _fileManager = new(FILEMANAGER_WIDTH, WindowHeight, initialFilePath);
        _richPresence = new();

        PlaytimeCounter playtimeCounter = new();
        _config = new();
        _terminal = new();

        _permutables = [
            _editor,
            _fileManager,
            playtimeCounter,
            _terminal
        ];

        _renderables = [
            _editor,
            _activityBar,
            _fileManager,
            _config,
            _terminal
        ];

        _disposables = [
            _window,
            _renderer,
            _richPresence,
            playtimeCounter
        ];

        _activityProvider = _editor;
        _focusedTextEditor = _editor;
    }

    public void Update(double deltaTime)
    {
        _fileManager.Height = WindowHeight;

        if (Input.IsLeftMousePressed())
        {
            if (Input.MouseX > WindowWidth - FILEMANAGER_WIDTH)
            {
                string? entry = _fileManager.HandleClick();
                if (entry != null)
                {
                    if (File.Exists(entry))
                    {
                        _editor.OpenFile(entry);
                    }
                    else if (Directory.Exists(entry))
                    {
                        _fileManager.OpenDirectory(entry);
                    }
                    else
                    {
                        throw new Exception("Entry somehow was neither a file nor a directory");
                    }
                }
            }
        }

        if (Input.IsKeyPressed(SDL.Keycode.F11))
        {
            SDL.SetWindowFullscreen(_window.Handle, !_windowFullscreen);
            _windowFullscreen = !_windowFullscreen;
        }

        if (Input.IsKeyDown(SDL.Keycode.LCtrl))
        {
            if (Input.IsKeyPressed(SDL.Keycode.Comma))
            {
                SwitchEditorAndConfig();
            }

            if (Input.IsKeyPressed(SDL.Keycode.T))
            {
                _terminal.Opened = !_terminal.Opened;

                if (_terminal.Opened)
                    _focusedTextEditor = _terminal;
                else
                    _focusedTextEditor = _editor;
            }
        }

        _activityBar.SetActivity(_activityProvider.ProvideActivity());

        _focusedTextEditor?.TextEdit(_keybindSet);

        foreach (IPermutable permutable in _permutables)
            permutable.Update(deltaTime);
    }

    public void Render()
    {
        _renderer.Clear(Drip.BaseColor.ToColor());

        foreach (IRenderable renderable in _renderables)
            if (renderable.ShouldRender) renderable.Render(_renderer);

        _renderer.RenderPresent();

        PointSizeDeleter.Flush();
        if (_scheduledPointSizeDeletions.Count > 0)
        {
            _renderer.RefreshTextEngine();
            FontManager.Reload(_renderer);
        }
    }

    public void End()
    {
        foreach (IDisposable disposable in _disposables)
            disposable.Dispose();

        SDL.StopTextInput(_window.Handle);
        AssetManager.Dispose();
    }

    public void SetWindowSize(int width, int height)
    {
        WindowWidth = width;
        WindowHeight = height;
    }

    private void SwitchEditorAndConfig()
    {
        if (_config.ShouldRender)
        {
            _config.ShouldRender = false;
            _editor.ShouldRender = true;
            _activityProvider = _editor;
        }
        else
        {
            _config.ShouldRender = true;
            _editor.ShouldRender = false;
            _activityProvider = _config;
        }
    }
}
