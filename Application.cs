using SDL3;
using SmashFramework;

using Color = System.Drawing.Color;

public class App : Application
{
    public const string FONT_NAME = "Rubik-Regular";
    public const int POINT_SIZE = 24;

    public static Font Font = null!;

    public static readonly Color BackgroundColor = Color.FromArgb(255, 20, 20, 20);

    private readonly Window _window;
    private readonly Renderer _renderer;

    private readonly Editor _editor;

    public App()
    {
        CreateWindowAndRenderer("Sharpon!", 800, 600, out _window, out _renderer);
        SDL.StartTextInput(_window.Handle);

        AssetManager.SetAssetRootDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets"));
        AssetManager.LoadFont("Rubik-Regular.ttf");

        Font = AssetManager.Get<Font>(FONT_NAME);

        _renderer.SetVSyncEnabled(true);
        _renderer.SetRenderBlendMode(BlendMode.Blend);

        _editor = new("Text der zum testen gedacht ist");
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
