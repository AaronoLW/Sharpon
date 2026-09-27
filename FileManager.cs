using System.Drawing;
using System.Numerics;
using SmashFramework;

public class FileManager
{
    private const float ENTRY_SPACING = App.VERY_SMALL_POINT_SIZE * 2;

    public float Width;
    public float Height;

    private Vector2 Position => new(App.WindowWidth - Width, 0);

    private readonly PathManager _pathManager = new("No directory opened");

    private readonly string[]? _fileSystemEntries;

    public FileManager(int width, int height)
    {
        Width = width;
        Height = height;

        _fileSystemEntries = _pathManager.OpenDirectory(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
    }

    public void Render(Renderer renderer)
    {
        renderer.RenderFilledRectangle(new(Position, Width, Height), App.BackgroundColor);
        renderer.RenderLine(Position, Position with { Y = App.WindowHeight }, App.VeryLightColor);

        string directoryName = _pathManager.DisplayPath[(_pathManager.DisplayPath.LastIndexOf('/') + 1).._pathManager.DisplayPath.Length];
        renderer.RenderText(App.Font, App.SMALL_POINT_SIZE, directoryName, Position + new Vector2(App.DEFAULT_PADDING), Color.White);

        if (_fileSystemEntries != null)
        {
            Vector2 basePosition = Position + new Vector2(App.DEFAULT_PADDING * 1.2f, App.DEFAULT_PADDING * 1.5f);

            for (int i = 0; i < _fileSystemEntries.Length; i++)
            {
                Vector2 position = basePosition + new Vector2(0, (i + 1) * ENTRY_SPACING);

                string name = Path.GetFileName(_fileSystemEntries[i]);
                Color color = Directory.Exists(_fileSystemEntries[i]) ? Color.RoyalBlue : Color.White;

                if (Directory.Exists(_fileSystemEntries[i]))
                    name += Path.DirectorySeparatorChar;

                renderer.RenderText(App.SmallFont, App.VERY_SMALL_POINT_SIZE, name, position, color);
            }
        }
    }
}
