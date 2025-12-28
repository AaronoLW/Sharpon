using System;
using Microsoft.Xna.Framework.Input;

public class UpArrowCommand : IKeyCommand
{
    public bool RequiresControl { get; } = false;
    public bool Repeats { get; } = true;
    public Keys AssignedKey { get; } = Keys.Up;

    public void Execute(UIElement uiElement)
    {
        for (int i = uiElement.CharIndex - 1; i > 0; i--)
        {
            if (uiElement.Text[i] == '\n') 
            {
                uiElement.SetCharIndex(i);
                return;
            };
        }
    }
}