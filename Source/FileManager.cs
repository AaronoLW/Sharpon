using System.Numerics;
using SmashFramework;
using SDL3;
using KatziDrip;
using Color = System.Drawing.Color;

public class FileManager : IPermutable, IRenderable
{
    private const float ENTRY_SPACING = App.VERY_SMALL_POINT_SIZE * 2;
    private const float AUTO_REFRESH_INTERVAL = 0.5f;

    public bool Opened = true;

    public float Width;
    public float Height;

    private Vector2 Position => new(App.WindowWidth - Width, 0);
    private Vector2 EntryBasePosition => Position + new Vector2(App.DEFAULT_PADDING * 1.2f, App.DEFAULT_PADDING * 2) - new Vector2(0, _scroll.Scroll);

    private PathManager _pathManager = new("No directory opened");
    private readonly Scroller _scroll = new(0, null);

    private const int BACK_BUTTON_SIZE = 40;
    private Rectangle _backButtonRectangle => new(new Vector2(App.WindowWidth - App.DEFAULT_PADDING / 1.5f - BACK_BUTTON_SIZE - (Width - App.MAX_FILEMANAGER_WIDTH), App.DEFAULT_PADDING / 1.5f), BACK_BUTTON_SIZE, BACK_BUTTON_SIZE);
    private Kolor _backButtonColor = Drip.LightColor;

    private string[]? _fileSystemEntries;

    private float _autoRefreshTimer = AUTO_REFRESH_INTERVAL;

    public bool ShouldRender { get; set; } = true;

    private Window? _contextWindow = null;
    private Renderer? _contextRenderer = null;
    private int? _contextSelectedEntry = null;

    private Window _window;

    public FileManager(int width, int height, string? initialPath, Window window)
    {
        Width = width;
        Height = height;
        _window = window;

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

    public void Update(double deltaTime)
    {
        if (Input.MouseX > App.WindowWidth - Width)
        {
            _scroll.Update();
            _scroll.Clamp();

            if (Input.IsRightMousePressed())
            {
                OpenContextMenu(_window);
            }
        }

        if (_contextWindow != null && SDL.GetMouseFocus() == _contextWindow.Handle)
        {
            if (Input.IsLeftMousePressed())
            {
                int? entry = GetHoveredContextEntry();
                if (entry.HasValue)
                {
                    Console.WriteLine(GetContextEntryText(entry.Value));
                }

                CloseContextMenu();
            }
        }

        _scroll.Animate(deltaTime);

        if (Opened)
        {
            Width = MathHelper.Larp(Width, App.MAX_FILEMANAGER_WIDTH, App.ANIMATION_SPEED * (float)deltaTime);
        }
        else
        {
            Width = MathHelper.Larp(Width, 0, App.ANIMATION_SPEED * (float)deltaTime);
        }

        if (_backButtonRectangle.IsPositionInRectangle(Input.MousePosition))
        {
            _backButtonColor = new(255, 255, 255, 255);
            if (Input.IsLeftMousePressed())
            {
                string? parentDirectory = GetParentDirectory();
                if (parentDirectory != null)
                    OpenDirectory(parentDirectory);
            }
        }
        else
        {
            _backButtonColor = Drip.VeryLightColor;
        }

        _autoRefreshTimer -= (float)deltaTime;
        if (_autoRefreshTimer <= 0)
        {
            OpenDirectory(_pathManager.Path, false);
            _autoRefreshTimer = AUTO_REFRESH_INTERVAL;
        }
    }

    public void Render(Renderer renderer)
    {
        // This really is just starting to look like zed

        renderer.RenderFilledRectangle(new(Position, Width, Height), Drip.DarkColor.ToColor());
        renderer.RenderLine(Position, Position with { Y = App.WindowHeight }, Drip.VeryLightColor.ToColor());

        string directoryName = _pathManager.DisplayPath;

        if (directoryName.Length != 1)
            directoryName = _pathManager.DisplayPath[(_pathManager.DisplayPath.LastIndexOf(Path.DirectorySeparatorChar) + 1).._pathManager.DisplayPath.Length];

        renderer.RenderString(FontManager.FontRegular, App.SMALL_POINT_SIZE, directoryName, Position + new Vector2(App.DEFAULT_PADDING), Drip.White.ToColor());

        renderer.RenderFilledRectangle(_backButtonRectangle, Drip.LightColor.ToColor());
        renderer.RenderRectangle(_backButtonRectangle, _backButtonColor.ToColor());

        Vector2 buttonTextSize = FontManager.FontRegular.MeasureString("<", App.SMALL_POINT_SIZE);
        renderer.RenderString(FontManager.FontRegular, App.SMALL_POINT_SIZE, "<", Vector2.Round(_backButtonRectangle.Position + _backButtonRectangle.Size / 2 - buttonTextSize / 2), Drip.White.ToColor());

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

                Kolor color;
                if (Directory.Exists(_fileSystemEntries[i]))
                {
                    color = Drip.PrimaryAccentColor;
                }
                else
                {
                    color = Drip.White;
                    if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS() || OperatingSystem.IsFreeBSD())
                    {
                        if ((File.GetUnixFileMode(_fileSystemEntries[i]) & (UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute)) != 0)
                        {
                            color = Drip.SecondaryAccentColor;
                        }
                    }
                    else if (OperatingSystem.IsWindows())
                    {
                        if (Path.GetExtension(_fileSystemEntries[i]).Equals(".exe", StringComparison.OrdinalIgnoreCase))
                        {
                            color = Drip.SecondaryAccentColor;
                        }
                    }
                }

                if (Directory.Exists(_fileSystemEntries[i]))
                    name += Path.DirectorySeparatorChar;

                if (hoveredEntry == i || _contextSelectedEntry == i)
                    renderer.RenderFilledRectangle(GetEntryRectangle(i), Drip.VeryLightColor.ToColor());

                renderer.RenderString(FontManager.FontRegular, App.VERY_SMALL_POINT_SIZE, name, position, color.ToColor());
            }

            SDL.SetRenderClipRect(renderer.Handle, 0);
        }

        if (_contextWindow != null && _contextRenderer != null)
        {
            _contextRenderer.Clear(Drip.BaseColor.ToColor());
            _contextRenderer.RenderRectangle(new(0, 0, _contextWindow.Width, _contextWindow.Height), Drip.VeryLightColor.ToColor());

            float entryHeight = 40;

            for (int i = 0; i < 5; i++)
            {
                Vector2 position = GetContextEntryPosition(i);
                _contextRenderer.RenderLine(position + new Vector2(0, entryHeight), position + new Vector2(150, entryHeight), Drip.VeryLightColor.ToColor());

                if (GetHoveredContextEntry() == i)
                {
                    _contextRenderer.RenderFilledRectangle(new(position, new Vector2(150, entryHeight)), Drip.VeryLightColor.ToColor());
                }

                string text = GetContextEntryText(i);
                Vector2 textSize = FontManager.FontRegular.MeasureString(text, 17);
                _contextRenderer.RenderString(FontManager.FontRegular, 17, text, position + new Vector2(App.DEFAULT_PADDING / 2, textSize.Y / 2), Color.White);
            }

            _contextRenderer.RenderPresent();
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

    private Vector2 GetContextEntryPosition(int index)
    {
        float entryHeight = 40;
        return new(0, index * entryHeight);
    }

    private int? GetHoveredContextEntry()
    {
        for (int i = 0; i < 5; i++)
        {
            Rectangle rectangle = new(GetContextEntryPosition(i), 150, 40);

            if (rectangle.IsPositionInRectangle(Input.MousePosition))
            {
                return i;
            }
        }

        return null;
    }

    private string GetContextEntryText(int index)
    {
        if (_contextSelectedEntry != null)
        {
            return index switch
            {
                0 => "New File",
                1 => "New Directory",
                2 => "Delete",
                3 => "Rename",
                _ => ""
            };
        }
        else
        {
            return index switch
            {
                0 => "New File",
                1 => "New Directory",
                _ => ""
            };
        }
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

    private string? GetParentDirectory()
    {
        if (_pathManager.Path == null)
            return null;

        DirectoryInfo? parent = Directory.GetParent(_pathManager.Path);
        if (parent == null)
            return null;

        return parent.FullName;
    }

    private void OpenContextMenu(Window mainWindow)
    {
        if (_contextWindow != null)
        {
            _contextWindow.Dispose();
            _contextRenderer!.Dispose();
        }

        _contextWindow = new(SDL.CreatePopupWindow(mainWindow.Handle, (int)Input.MouseX, (int)Input.MouseY, 150, 200, SDL.WindowFlags.PopupMenu));
        _contextRenderer = new(_contextWindow);
        _contextSelectedEntry = GetHoveredEntryIndex();
    }

    private void CloseContextMenu()
    {
        if (_contextWindow != null)
        {
            _contextWindow.Dispose();
            _contextRenderer!.Dispose();

            _contextWindow = null;
            _contextRenderer = null;

            _contextSelectedEntry = null;
        }
    }

    public void TryCloseContextMenu()
    {
        if (_contextWindow != null)
        {
            if (_contextWindow.Handle == SDL.GetMouseFocus())
                return;

            CloseContextMenu();
        }
    }

    public void OpenDirectory(string? path, bool resetScroll = true)
    {
        _fileSystemEntries = _pathManager.OpenDirectory(path);
        if (resetScroll) _scroll.Reset();
    }
}
