using KatziDrip;
using SmashFramework;

public static class ColorTranslator
{
    public static Color Get(Kolor kolor)
    {
        return new((byte)kolor.R, (byte)kolor.G, (byte)kolor.B, (byte)kolor.A);
    }

    public static Color Get(System.Drawing.Color color)
    {
        return new(color.R, color.G, color.B, color.A);
    }
}
