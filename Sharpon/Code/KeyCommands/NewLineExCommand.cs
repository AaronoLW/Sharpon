using System;
using Microsoft.Xna.Framework.Input;

public class NewLineExCommand : KeyCommand
{
    public override bool RequiresControl { get; } = true;
    public override bool RequiresShift { get; } = false;
    public override bool Repeats { get; } = false;
    public override Keys AssignedKey { get; } = Keys.Enter;
    public override Action<UIElement> _customBehaviour { get; protected set; }

    public NewLineExCommand(Action<UIElement> customBehaviour = null) => _customBehaviour = customBehaviour; 
    
    protected override void DefaultBehaviour(UIElement uiElement)
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