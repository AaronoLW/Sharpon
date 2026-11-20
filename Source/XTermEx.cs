using SmashFramework;
using XTerm.Common;

public static class XTermEx
{
    public static Color XtermColor(int index)
    {
        Color[] ansiColors =
        [
            new(0, 0, 0),
            new(205, 0, 0),
            new(0, 205, 0),
            new(205, 205, 0),
            new(0, 0, 238),
            new(205, 0, 205),
            new(0, 205, 205),
            new(229, 229, 229),
            new(127, 127, 127),
            new(255, 0, 0),
            new(0, 255, 0),
            new(255, 255, 0),
            new(92, 92, 255),
            new(255, 0, 255),
            new(0, 255, 255),
            new(255, 255, 255)
        ];

        if (index < 0 || index > 255)
            throw new ArgumentOutOfRangeException(nameof(index));

        if (index < 16)
            return ansiColors[index];

        if (index < 232)
        {
            int n = index - 16;

            int r = n / 36;
            int g = (n / 6) % 6;
            int b = n % 6;

            int[] levels = [0, 95, 135, 175, 215, 255];

            return new(
                (byte)levels[r],
                (byte)levels[g],
                (byte)levels[b]);
        }

        int gray = 8 + (index - 232) * 10;

        return new((byte)gray, (byte)gray, (byte)gray);
    }

    public static CaretStyle ToCaretStyle(CursorStyle cursorStyle)
    {
        return cursorStyle switch
        {
            CursorStyle.Block => CaretStyle.Block,
            CursorStyle.Bar => CaretStyle.Beam,
            CursorStyle.Underline => CaretStyle.Underline,
            _ => CaretStyle.Beam
        };
    }
}
