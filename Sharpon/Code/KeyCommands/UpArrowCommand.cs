using System;
using Microsoft.Xna.Framework.Input;

public class UpArrowCommand : KeyCommand
{
    public override bool RequiresControl { get; } = false;
    public override bool RequiresShift { get; } = false;
    public override bool Repeats { get; } = true;
    public override Keys AssignedKey { get; } = Keys.Up;
    public override Action<UIElement> _customBehaviour { get; protected set; }

    public UpArrowCommand(Action<UIElement> customBehaviour = null) => _customBehaviour = customBehaviour; 

    protected override void DefaultBehaviour(UIElement uiElement)
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