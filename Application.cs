using SmashFramework;

using Color = System.Drawing.Color;

public class App : Application
{
    private readonly Window _window;
    private readonly Renderer _renderer;

    public App()
    {
        CreateWindowAndRenderer("Sharpon!", 800, 600, out _window, out _renderer);

        AssetManager.SetAssetRootDirectory(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets"));

        _renderer.SetVSyncEnabled(true);
        _renderer.SetRenderBlendMode(BlendMode.Blend);
    }

    public override void Update(double deltaTime)
    {

    }

    public override void Render()
    {
        _renderer.Clear(Color.CornflowerBlue);

        _renderer.RenderPresent();
    }

    public override void End()
    {
        _window.Dispose();
        _renderer.Dispose();
        AssetManager.Dispose();
    }
}
