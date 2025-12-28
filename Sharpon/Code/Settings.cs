using Microsoft.Xna.Framework;

public static class Settings
{
    public static readonly int FontSize = 25;
    public static readonly int Spacing = 24;
    public static readonly float ArrowKeyBaseCooldown = 0.15f;
    public static readonly float ArrowKeyBaseFastCooldown = 0.02f;
    public static Vector2 SpacingVector => new Vector2(0, Spacing);

    public static readonly char[] JumpStopChars = { ' ', '.', ',', '/', '(', '{', '[', '<' };
}