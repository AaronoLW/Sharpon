using System.Drawing;
using System.Numerics;
using SmashFramework;

public class ActivityBar : IRenderable
{
    private string _mainText = "";
    private string _subText = "";

    public bool ShouldRender { get; set; } = true;

    public void Render(Renderer renderer)
    {
        renderer.RenderFilledRectangle(new(0, 0, App.WindowWidth, App.ACTIVITY_BAR_HEIGHT), App.BackgroundColor);
        renderer.RenderLine(new(0, App.ACTIVITY_BAR_HEIGHT), new(App.WindowWidth, App.ACTIVITY_BAR_HEIGHT), App.VeryLightColor);

        Vector2 textSize = App.Font.MeasureString(_mainText, App.SMALL_POINT_SIZE);
        Vector2 textPosition = new(App.DEFAULT_PADDING, (App.ACTIVITY_BAR_HEIGHT / 2) - (textSize.Y / 2.5f));
        renderer.RenderText(App.Font, App.SMALL_POINT_SIZE, _mainText, textPosition, Color.White);
    }

    public void SetActivity(Activity activity)
    {
        _mainText = activity.MainText;
        _subText = activity.SubText;
    }
}
