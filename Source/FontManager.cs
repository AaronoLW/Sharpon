using SmashFramework;
using KatziDrip;

public static class FontManager
{
    public static Font FontRegular { get; private set; } = null!;

    public static void Init(Renderer renderer)
    {
        string[]? fontPaths = SystemFontResolver.ResolveByFamily(Drip.FontFamily);

        if (fontPaths != null)
        {
            foreach (string fontPath in fontPaths)
            {
                if (fontPath.EndsWith("-Medium.ttf"))
                {
                    Console.WriteLine($"Fontpath: {fontPath}");
                    FontRegular = AssetManager.LoadFont(fontPath, renderer);
                    return;
                }
            }
        }

        string assetFontPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", $"{Drip.FontFamily}-Medium.ttf");
        if (File.Exists(assetFontPath))
        {
            Console.WriteLine($"Fontpath: {assetFontPath}");
            FontRegular = AssetManager.LoadFont(assetFontPath, renderer);
            return;
        }

        throw new Exception($"Font family {Drip.FontFamily} could not be found or is not installed");
    }

    public static void Reload(Renderer renderer)
    {
        FontRegular.Dispose();
        AssetManager.Remove(Path.GetFileNameWithoutExtension(FontRegular.Path));
        FontRegular = AssetManager.LoadFont(FontRegular.Path, renderer);
    }
}
