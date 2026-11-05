using System.Drawing;
using SmashFramework;

public class Config : IActivityProvider, IRenderable, IPermutable
{
    public bool ShouldRender { get; set; } = false;

    private readonly Scroller _scroll = new(0, null);
    private float _previousScroll = 0;

    private readonly GUIRoot _guiRoot = new(
            App.ActivityPosition,
            new TextElement("CONFIGGGGGGG", FontManager.FontRegular, App.BIG_POINT_SIZE, Color.White),
            new TextElement("CONFIGGGGGGG", FontManager.FontRegular, App.POINT_SIZE, Color.White)
    );

    public void Update(double deltaTime)
    {
        if (!ShouldRender)
            return;

        _scroll.Update();
        _scroll.Clamp();
        _scroll.Animate(deltaTime);

        if (_scroll.Scroll != _previousScroll)
        {
            _previousScroll = _scroll.Scroll;
            _guiRoot.Y = App.ActivityPosition.Y - _scroll.Scroll;
            _guiRoot.RecalculatePositions();
        }
    }

    public void Render(Renderer renderer)
    {
        _guiRoot.Render(renderer);
    }

    public Activity ProvideActivity()
    {
        return new("Config", "");
    }
}
