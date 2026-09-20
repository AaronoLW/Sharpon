using SDL3;
using SmashFramework;

using Color = System.Drawing.Color;

public class App : Application
{
    private const string DEFAULT_TEXT = "Text der zum testen gedacht ist (Raphi edition)";

    public const string FONT_NAME = "Rubik-Regular";
    public const int POINT_SIZE = 24;

    public static Font Font = null!;

    public static readonly Color BackgroundColor = Color.FromArgb(255, 20, 20, 20);

    private readonly Window _window;
    private readonly Renderer _renderer;

    private readonly Editor _editor;

    public App(string? initialFilePath)
    {
        CreateWindowAndRenderer("Sharpon!", 800, 600, out _window, out _renderer);
        _window.SetWindowResizable(true);
        SDL.StartTextInput(_window.Handle);

        AssetManager.SetAssetRootDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets"));
        AssetManager.LoadFont("Rubik-Regular.ttf");

        Font = AssetManager.Get<Font>(FONT_NAME);

        _renderer.SetVSyncEnabled(true);
        _renderer.SetRenderBlendMode(BlendMode.Blend);

        string initialText = DEFAULT_TEXT;
        if (initialFilePath != null)
        {
            string fullFilePath = Path.GetFullPath(initialFilePath);
            initialText = File.Exists(fullFilePath) ? File.ReadAllText(fullFilePath) : DEFAULT_TEXT;
            Console.WriteLine($"Opening file at: {fullFilePath}");
        }

        _editor = new(initialText);
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
}
