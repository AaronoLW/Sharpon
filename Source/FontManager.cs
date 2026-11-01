using SmashFramework;
using KatziDrip;

public static class FontManager
{
    public const string FontId = "Font";
    public const string FontName = "Iosevka-Medium";

    public static Font FontRegular { get; private set; } = null!;
    public static readonly string FontSize = "Medium";

    private static Stream FontStream => GetFontStream();

    public static void Init(Renderer renderer)
    {
        FontRegular = AssetManager.LoadFont(FontId, FontStream, renderer);
    }

    public static void Reload(Renderer renderer)
    {
        FontRegular.Dispose();
        AssetManager.Remove(Path.GetFileNameWithoutExtension(FontId));
        FontRegular = AssetManager.LoadFont(FontId, FontStream, renderer);
    }

    private static Stream GetFontStream()
    {
        Stream? embeddedStream = typeof(FontManager).Assembly.GetManifestResourceStream($"Sharpon.Assets.{FontName}.ttf");

        if (embeddedStream != null)
            return embeddedStream;

        string[]? fontPaths = SystemFontResolver.ResolveByFamily(Drip.FontFamily);

        if (fontPaths != null)
        {
            foreach (string fontPath in fontPaths)
            {
                if (fontPath.EndsWith($"-{FontSize}.ttf"))
                {
                    return File.OpenRead(fontPath);
                }
            }
        }

        string assetFontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", $"{Drip.FontFamily}-{FontSize}.ttf");
        if (File.Exists(assetFontPath))
        {
            Console.WriteLine($"Fontpath: {assetFontPath}");
            return File.OpenRead(assetFontPath);
        }

        throw new Exception($"Font family {Drip.FontFamily} could not be found or is not installed");
    }
}
