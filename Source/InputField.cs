using System.Drawing;
using System.Numerics;
using SmashFramework;

public class InputField
{
    public string Text = "";
    public string Prefix = "";

    public float X;
    public float Y;
    public Vector2 Position
    {
        get => new(X, Y);
        set { X = value.X; Y = value.Y; }
    }

    public float Width;
    public float Height;
    public Vector2 Size
    {
        get => new(Width, Height);
        set { Width = value.X; Height = value.Y; }
    }

    public void Render(Renderer renderer)
    {
        float xOffset = 0;

        if (Prefix != string.Empty)
        {
            Vector2 prefixTextSize = FontManager.FontRegular.MeasureString(Prefix, App.POINT_SIZE);
            Vector2 prefixPosition = Position + new Vector2(App.DEFAULT_PADDING, (Height / 2) - (prefixTextSize.Y / 2));
            renderer.RenderString(FontManager.FontRegular, App.POINT_SIZE, Prefix, Vector2.Round(prefixPosition), Color.White);
            xOffset += prefixTextSize.X;
        }

        Vector2 textSize = FontManager.FontRegular.MeasureString(Text, App.POINT_SIZE);
        Vector2 textPosition = Position + new Vector2(xOffset * 2, 0) + new Vector2(App.DEFAULT_PADDING, (Height / 2) - (textSize.Y / 2));
        renderer.RenderString(FontManager.FontRegular, App.POINT_SIZE, Text, Vector2.Round(textPosition), Color.White);
    }
}
