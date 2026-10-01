using Color = System.Drawing.Color;
using System.Numerics;
using SmashFramework;
using SDL3;

public class FileManager
{
    private const float ENTRY_SPACING = App.VERY_SMALL_POINT_SIZE * 2;

    public float Width;
    public float Height;

    private Vector2 Position => new(App.WindowWidth - Width, 0);
    private Vector2 EntryBasePosition => Position + new Vector2(App.DEFAULT_PADDING * 1.2f, App.DEFAULT_PADDING * 1.5f) - new Vector2(0, _scroll.Scroll);

    private PathManager _pathManager = new("No directory opened");
    private readonly Scroller _scroll = new(0, null);

    private string[]? _fileSystemEntries;

    public FileManager(int width, int height, string? initialPath)
    {
        Width = width;
        Height = height;

        OpenDirectory(initialPath);
    }

    public string? HandleClick()
    {
        if (_fileSystemEntries != null)
        {
            for (int i = 0; i < _fileSystemEntries.Length; i++)
            {
                Rectangle rect = GetEntryRectangle(i);

                if (rect.IsPositionInRectangle(Input.MousePosition))
                    return _fileSystemEntries[i];
            }
        }

        return null;
    }

    public void Update()
    {
        _scroll.Update();

        if (_scroll.Scroll < 0)
            _scroll.Reset();
    }

    public void Animate(double deltaTime)
    {
        _scroll.Animate(deltaTime);
    }

    public void Render(Renderer renderer)
    {
        // This really is just starting to look like zed

        renderer.RenderFilledRectangle(new(Position, Width, Height), App.DarkColor);
        renderer.RenderLine(Position, Position with { Y = App.WindowHeight }, App.VeryLightColor);

        string directoryName = _pathManager.DisplayPath[(_pathManager.DisplayPath.LastIndexOf(Path.DirectorySeparatorChar) + 1).._pathManager.DisplayPath.Length];
        renderer.RenderText(App.Font, App.SMALL_POINT_SIZE, directoryName, Position + new Vector2(App.DEFAULT_PADDING), Color.White);

        if (_fileSystemEntries != null)
        {
            int? hoveredEntry = GetHoveredEntryIndex();

            Vector2 clipRectanglePosition = EntryBasePosition;
            clipRectanglePosition.Y += _scroll.Scroll;
            clipRectanglePosition.Y += App.DEFAULT_PADDING;
            clipRectanglePosition.X = 0;

            Rectangle clipRect = new(clipRectanglePosition, App.WindowSize);
            SDL.SetRenderClipRect(renderer.Handle, clipRect.ToSDLRect());

            int startIndex = Math.Max((int)(_scroll.Scroll / ENTRY_SPACING), 0);
            for (int i = startIndex; i < _fileSystemEntries.Length; i++)
            {
                Vector2 position = EntryBasePosition + GetEntryOffset(i);

                if (position.Y > App.WindowHeight)
                    break;

                string name = Path.GetFileName(_fileSystemEntries[i]);
                Color color = Directory.Exists(_fileSystemEntries[i]) ? Color.RoyalBlue : Color.White;

                if (Directory.Exists(_fileSystemEntries[i]))
                    name += Path.DirectorySeparatorChar;

                if (hoveredEntry == i)
                    renderer.RenderFilledRectangle(GetEntryRectangle(i), App.VeryLightColor);

                renderer.RenderText(App.SmallFont, App.VERY_SMALL_POINT_SIZE, name, position, color);
            }

            SDL.SetRenderClipRect(renderer.Handle, 0);
        }
    }

    private Vector2 GetEntryOffset(int index)
    {
        return new Vector2(0, (index + 1) * ENTRY_SPACING);
    }

    private Rectangle GetEntryRectangle(int index)
    {
        Vector2 entryPosition = EntryBasePosition + GetEntryOffset(index);
        entryPosition.X = App.WindowWidth - Width;
        entryPosition.Y -= ENTRY_SPACING / 4;

        return new(entryPosition, Width, ENTRY_SPACING);
    }

    private int? GetHoveredEntryIndex()
    {
        if (_fileSystemEntries == null)
            return null;

        for (int i = 0; i < _fileSystemEntries.Length; i++)
        {
            Rectangle rectangle = GetEntryRectangle(i);

            if (rectangle.IsPositionInRectangle(Input.MousePosition))
            {
                return i;
            }
        }

        return null;
    }

    public void OpenDirectory(string? path)
    {
        _fileSystemEntries = _pathManager.OpenDirectory(path);
        _scroll.Reset();
    }
}
