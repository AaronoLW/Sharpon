using System;
using Microsoft.Xna.Framework;
using Sharpon;

public class Editor : UIElement
{
    public Editor(UISystem uiSystem, Vector2 position, string text, Color backgroundColor) : base(uiSystem, position, text, backgroundColor)
    {
        _keybindHandler = new KeybindHandler(new NewLineCommand(),
                                             new NewLineExCommand(),
                                             new BackspaceCommand(),
                                             new BackspaceExCommand(),
                                             new LeftArrowCommand(),
                                             new RightArrowCommand(),
                                             new UpArrowCommand(),
                                             new DownArrowCommand());
    }
    
    public Vector2 CalculateCaretPosition(int charIndex)
    {
        string[] lines = Text.Substring(0, charIndex).Split("\n");
        return new Vector2(X + Game1.Font.MeasureString(lines[^1]).X - (Game1.Font.MeasureString("|").X / 2), Y + ((lines.Length - 1) * Settings.Spacing));
    }
}