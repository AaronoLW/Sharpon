using System.Drawing;
using XTerm.Common;

public static class XTermEx
{
    public static Color XtermColor(int index)
    {
        Color[] ansiColors =
        [
            Color.FromArgb(0, 0, 0),
            Color.FromArgb(205, 0, 0),
            Color.FromArgb(0, 205, 0),
            Color.FromArgb(205, 205, 0),
            Color.FromArgb(0, 0, 238),
            Color.FromArgb(205, 0, 205),
            Color.FromArgb(0, 205, 205),
            Color.FromArgb(229, 229, 229),
            Color.FromArgb(127, 127, 127),
            Color.FromArgb(255, 0, 0),
            Color.FromArgb(0, 255, 0),
            Color.FromArgb(255, 255, 0),
            Color.FromArgb(92, 92, 255),
            Color.FromArgb(255, 0, 255),
            Color.FromArgb(0, 255, 255),
            Color.FromArgb(255, 255, 255)
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

            return Color.FromArgb(
                levels[r],
                levels[g],
                levels[b]);
        }

        int gray = 8 + (index - 232) * 10;

        return Color.FromArgb(gray, gray, gray);
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
