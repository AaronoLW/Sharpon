using System.Drawing;
using System.Numerics;
using SmashFramework;

public class FileManager
{
    public float Width;
    public float Height;

    private Vector2 Position => new(App.WindowWidth - Width, 0);

    private readonly PathManager _pathManager = new("No directory opened");

    public FileManager(int width, int height)
    {
        Width = width;
        Height = height;

        _pathManager.OpenDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    }

    public void Render(Renderer renderer)
    {
        renderer.RenderFilledRectangle(new(Position, Width, Height), App.BackgroundColor);
        renderer.RenderLine(Position, Position with { Y = App.WindowHeight }, App.VeryLightColor);

        string directoryName = _pathManager.DisplayPath[(_pathManager.DisplayPath.LastIndexOf('/') + 1).._pathManager.DisplayPath.Length];

        renderer.RenderText(App.Font, App.SMALL_POINT_SIZE, directoryName, Position + new Vector2(App.DEFAULT_PADDING), Color.White);
    }
}
