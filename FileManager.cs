using System.Drawing;
using System.Numerics;
using SmashFramework;

public class FileManager(float widthPercent, float heightPercent)
{
    public float WidthPercent = widthPercent;
    public float HeightPercent = heightPercent;

    public float Width => App.WindowWidth * WidthPercent;
    public float Height => App.WindowHeight * HeightPercent;

    private readonly PathManager _pathManager = new();

    public void Render(Renderer renderer)
    {
        Vector2 position = new(App.WindowWidth - Width, 0);
        renderer.RenderFilledRectangle(new(position, Width, Height), App.BackgroundColor);
        renderer.RenderLine(position, position with { Y = App.WindowWidth }, App.VeryLightColor);
    }
}
