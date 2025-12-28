using System;
using System.Linq;
using Microsoft.Xna.Framework.Input;

public class BackspaceExCommand : IKeyCommand
{
    public bool RequiresControl { get; } = true;
    public bool Repeats { get; } = true;
    public Keys AssignedKey { get; } = Keys.Back;

    public void Execute(UIElement uiElement)
    {
        for (int i = uiElement.CharIndex - 1; i > 0; i--)
        {
            if (Settings.JumpStopChars.Contains(uiElement.Text[i]) && uiElement.Text[i - 1] != ' ' || uiElement.Text[i - 1] == '\n')
            {
                uiElement.RemoveTextAt(uiElement.CharIndex, uiElement.CharIndex - i);
                uiElement.SetCharIndex(uiElement.CharIndex - (uiElement.CharIndex - i));
                return;
            }

            if (i - 1 == 0)
            {
                uiElement.RemoveTextAt(uiElement.CharIndex, uiElement.CharIndex - (i - 1));
                uiElement.SetCharIndex(uiElement.CharIndex - (uiElement.CharIndex - (i - 1)));
                return;
            }
        }
    }
}