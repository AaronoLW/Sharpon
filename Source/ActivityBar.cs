using System.Drawing;
using System.Numerics;
using SmashFramework;

public readonly struct ActivityBar(string mainText, string subText)
{
    public readonly string MainText = mainText;
    public readonly string SubText = subText;

    public void Render(Renderer renderer)
    {
        renderer.RenderFilledRectangle(new(0, 0, App.WindowWidth, App.ACTIVITY_BAR_HEIGHT), App.BackgroundColor);
        renderer.RenderLine(new(0, App.ACTIVITY_BAR_HEIGHT), new(App.WindowWidth, App.ACTIVITY_BAR_HEIGHT), App.VeryLightColor);

        Vector2 textSize = App.Font.MeasureString(MainText, App.SMALL_POINT_SIZE);
        Vector2 textPosition = new(App.DEFAULT_PADDING, (App.ACTIVITY_BAR_HEIGHT / 2) - (textSize.Y / 2.5f));
        renderer.RenderText(App.Font, App.SMALL_POINT_SIZE, MainText, textPosition, Color.White);
    }
}
