using System.Drawing;
using System.Numerics;
using SmashFramework;

public class FileManager(int width, int height)
{
    public float Width = width;
    public float Height = height;

    private readonly PathManager _pathManager = new();

    public void Render(Renderer renderer)
    {
        Vector2 position = new(App.WindowWidth - Width, 0);
        renderer.RenderFilledRectangle(new(position, Width, Height), App.BackgroundColor);
        renderer.RenderLine(position, position with { Y = App.WindowHeight }, App.VeryLightColor);
    }
}
