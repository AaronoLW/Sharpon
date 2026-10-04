using System.Numerics;
using KatziDrip;
using SmashFramework;

public class ActivityBar : IRenderable
{
    private string _mainText = "";
    private string _subText = "";

    public bool ShouldRender { get; set; } = true;

    public void Render(Renderer renderer)
    {
        renderer.RenderFilledRectangle(new(0, 0, App.WindowWidth, App.ACTIVITY_BAR_HEIGHT), Drip.BaseColor.ToSystemColor());
        renderer.RenderLine(new(0, App.ACTIVITY_BAR_HEIGHT), new(App.WindowWidth, App.ACTIVITY_BAR_HEIGHT), Drip.VeryLightColor.ToSystemColor());

        // Main text
        {
            Vector2 textSize = App.Font.MeasureString(_mainText, App.SMALL_POINT_SIZE);
            Vector2 textPosition = new(App.DEFAULT_PADDING, (App.ACTIVITY_BAR_HEIGHT / 2) - (textSize.Y / 2.5f));
            renderer.RenderText(App.Font, App.SMALL_POINT_SIZE, _mainText, textPosition, new Color(255, 255, 255, 255).ToSystemColor());
        }

        // Sub text
        {
            Vector2 textSize = App.Font.MeasureString(_subText, App.SMALL_POINT_SIZE);
            Vector2 textPosition = new(App.WindowWidth - App.FILEMANAGER_WIDTH - textSize.X - App.DEFAULT_PADDING, App.DEFAULT_PADDING);
            renderer.RenderText(App.Font, App.SMALL_POINT_SIZE, _subText, textPosition, new Color(255, 255, 255, 255).ToSystemColor());
        }
    }

    public void SetActivity(Activity activity)
    {
        _mainText = activity.MainText;
        _subText = activity.SubText;
    }
}
