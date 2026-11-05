using System.Drawing;
using SmashFramework;

public class TextElement(string text, Font font, float pointSize, Color textColor) : GUIElement
{
    public Font Font = font;
    public float PointSize = pointSize;
    public Color TextColor = textColor;

    public string Text = text;

    public override float Height => Font.MeasureString(Text, PointSize).Y;

    public override void Render(Renderer renderer)
    {
        renderer.RenderString(Font, PointSize, Text, Position, TextColor);
    }
}
