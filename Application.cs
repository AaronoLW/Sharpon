using System.Numerics;
using SDL3;
using SmashFramework;

using Color = System.Drawing.Color;

public class App : Application
{
    public const string FONT_NAME = "Roboto-Regular";
    public const int POINT_SIZE = 22;
    public const int SMALL_POINT_SIZE = 18;

    public const int DEFAULT_PADDING = 20;

    private const int INITIAL_WINDOW_WIDTH = 800;
    private const int INITIAL_WINDOW_HEIGHT = 600;

    public static int WindowWidth = INITIAL_WINDOW_WIDTH;
    public static int WindowHeight = INITIAL_WINDOW_HEIGHT;
    public static Vector2 WindowSize => new(WindowWidth, WindowHeight);

    public static Font Font = null!;

    public static readonly Color BackgroundColor = Color.FromArgb(255, 20, 20, 25);
    public static readonly Color LightColor = Color.FromArgb(255, 18, 18, 23);
    public static readonly Color VeryLightColor = Color.FromArgb(255, 45, 45, 50);

    private static Window _window = null!;
    private static Renderer _renderer = null!;

    private readonly Editor _editor;

    public App(string? initialFilePath)
    {
        CreateWindowAndRenderer("Sharpon!", INITIAL_WINDOW_WIDTH, INITIAL_WINDOW_HEIGHT, out _window, out _renderer);
        _window.SetWindowResizable(true);
        SDL.StartTextInput(_window.Handle);

        AssetManager.SetAssetRootDirectory(Path.GetTempPath());

        using (FileStream fileStream = File.Create(Path.Combine(Path.GetTempPath(), "Rubik-Regular.ttf")))
            fileStream.Write(AssetsGenerated.FontRubik);

        using (FileStream fileStream = File.Create(Path.Combine(Path.GetTempPath(), "Roboto-Regular.ttf")))
            fileStream.Write(AssetsGenerated.FontRoboto);

        AssetManager.LoadFont("Rubik-Regular.ttf");
        AssetManager.LoadFont("Roboto-Regular.ttf");

        Font = AssetManager.Get<Font>(FONT_NAME);

        _renderer.SetVSyncEnabled(true);
        _renderer.SetRenderBlendMode(BlendMode.Blend);

        _editor = new(initialFilePath);
    }

    public override void Update(double deltaTime)
    {
        _editor.Update(deltaTime);
    }

    public override void Render()
    {
        _renderer.Clear(BackgroundColor);

        _editor.Render(_renderer);

        _renderer.RenderPresent();
    }

    public override void End()
    {
        _window.Dispose();
        _renderer.Dispose();
        SDL.StopTextInput(_window.Handle);
        AssetManager.Dispose();
    }

    public void SetWindowSize(int width, int height)
    {
        WindowWidth = width;
        WindowHeight = height;
    }
}
