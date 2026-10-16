using SmashFramework;
using KatziDrip;

public static class FontManager
{
    public static Font FontRegular { get; private set; } = null!;

    public static void Init(Renderer renderer)
    {
        string[]? fontPaths = SystemFontResolver.ResolveByFamily(Drip.FontFamily);

        if (fontPaths == null)
            throw new Exception($"Font family {Drip.FontFamily} could not be found or is not installed");


        foreach (string fontPath in fontPaths)
        {
            if (fontPath.EndsWith("-Medium.ttf"))
            {
                FontRegular = AssetManager.LoadFont(fontPath, renderer);
                break;
            }
        }
    }

    public static void Reload(Renderer renderer)
    {
        FontRegular.Dispose();
        AssetManager.Remove(Path.GetFileNameWithoutExtension(FontRegular.Path));
        FontRegular = AssetManager.LoadFont(FontRegular.Path, renderer);
    }
}
