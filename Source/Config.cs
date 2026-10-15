using System.Drawing;
using SmashFramework;

public class Config : IActivityProvider, IRenderable
{
    public bool ShouldRender { get; set; } = false;

    public void Render(Renderer renderer)
    {
        renderer.RenderString(App.Font, App.POINT_SIZE, "Config!!!!!", App.ActivityPosition, Color.White);
    }

    public Activity ProvideActivity()
    {
        return new("Config", "");
    }
}
