using System.Numerics;
using SDL3;
using SmashFramework;

using Color = System.Drawing.Color;

public class App : Application
{
    public const string FONT_NAME = "Iosevka-Medium";
    public const string SMALL_FONT_NAME = "Iosevka-Regular";

    public const int POINT_SIZE = 21;
    public const int SMALL_POINT_SIZE = 18;
    public const int VERY_SMALL_POINT_SIZE = 16;

    public const int DEFAULT_PADDING = 20;

    private const int INITIAL_WINDOW_WIDTH = 800;
    private const int INITIAL_WINDOW_HEIGHT = 600;

    private const int FILEMANAGER_WIDTH = 250;

    public const int SCROLL_SPEED_AMPLIFIER = 20;

    public static readonly bool CSharpBracketStyle = true;

    public static int WindowWidth = INITIAL_WINDOW_WIDTH;
    public static int WindowHeight = INITIAL_WINDOW_HEIGHT;
    public static Vector2 WindowSize => new(WindowWidth, WindowHeight);

    public static Font Font = null!;
    public static Font SmallFont = null!;

    public static readonly Color BackgroundColor = Color.FromArgb(255, 20, 20, 25);
    public static readonly Color LightColor = Color.FromArgb(255, 18, 18, 23);
    public static readonly Color VeryLightColor = Color.FromArgb(255, 50, 50, 55);
    public static readonly Color DarkColor = Color.FromArgb(255, 15, 15, 20);

    private static Window _window = null!;
    private static Renderer _renderer = null!;

    private readonly Editor _editor;
    private readonly FileManager _fileManager;

    private readonly RichPresence _richPresence;

    public App(string? initialFilePath)
    {
        CreateWindowAndRenderer("Sharpon!", INITIAL_WINDOW_WIDTH, INITIAL_WINDOW_HEIGHT, out _window, out _renderer);
        _window.SetWindowResizable(true);
        SDL.StartTextInput(_window.Handle);

        string? fontPath = SystemFontResolver.Resolve(FONT_NAME);
        string? smallFontPath = SystemFontResolver.Resolve(SMALL_FONT_NAME);

        if (fontPath == null) throw new Exception($"Font {FONT_NAME} is not installed");
        if (smallFontPath == null) throw new Exception($"Font {SMALL_FONT_NAME} is not installed");

        AssetManager.LoadFont(fontPath);
        AssetManager.LoadFont(smallFontPath);

        Font = AssetManager.Get<Font>(FONT_NAME);
        SmallFont = AssetManager.Get<Font>(SMALL_FONT_NAME);

        _renderer.SetVSyncEnabled(true);
        _renderer.SetRenderBlendMode(BlendMode.Blend);

        _editor = new(initialFilePath);
        _fileManager = new(FILEMANAGER_WIDTH, WindowHeight, initialFilePath);
        _richPresence = new();
    }

    public override void Update(double deltaTime)
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

        if (Input.MouseX > WindowWidth - FILEMANAGER_WIDTH)
        {
            _fileManager.Update();
        }
        else
        {
            _editor.Update();
        }

        _editor.Animate(deltaTime);
        _fileManager.Animate(deltaTime);
    }

    public override void Render()
    {
        _renderer.Clear(BackgroundColor);

        _editor.Render(_renderer);
        _fileManager.Render(_renderer);

        _renderer.RenderPresent();
    }

    public override void End()
    {
        _window.Dispose();
        _renderer.Dispose();
        _richPresence.Dispose();
        SDL.StopTextInput(_window.Handle);
        AssetManager.Dispose();
    }

    public void SetWindowSize(int width, int height)
    {
        WindowWidth = width;
        WindowHeight = height;
    }
}
