using System;
using Microsoft.Xna.Framework.Input;

public class NewLineExCommand : IKeyCommand
{
    public bool RequiresControl { get; } = true;
    public bool Repeats { get; } = false;
    public Keys AssignedKey { get; } = Keys.Enter;
    
    public void Execute(UIElement uiElement)
    {
        for (int i = uiElement.CharIndex; i < uiElement.Text.Length; i++)
        {
            if (uiElement.Text[i] == '\n') 
            {
                uiElement.InsertTextAt(i, "\n");
                uiElement.SetCharIndex(i + 1);
                return;
            }

            if (i == uiElement.Text.Length - 1)
            {
                uiElement.InsertTextAt(i + 1, "\n");
                uiElement.SetCharIndex(uiElement.Text.Length);
                return;
            }
        }
    }
}