using System.Drawing;
using SmashFramework;

public class Config : IActivityProvider, IRenderable
{
    public bool ShouldRender { get; set; } = false;

    private readonly GUIRoot _guiRoot = new(
            App.ActivityPosition,
            new TextElement("CONFIGGGGGGG", FontManager.FontRegular, App.BIG_POINT_SIZE, Color.White),
            new TextElement("CONFIGGGGGGG", FontManager.FontRegular, App.POINT_SIZE, Color.White)
    );

    public void Render(Renderer renderer)
    {
        _guiRoot.Render(renderer);
    }

    public Activity ProvideActivity()
    {
        return new("Config", "");
    }
}
