using System;
using Microsoft.Xna.Framework.Input;

public class RightArrowCommand : KeyCommand
{
    public override bool RequiresControl { get; } = false;
    public override bool RequiresShift { get; } = false;
    public override bool Repeats { get; } = true;
    public override Keys AssignedKey { get; } = Keys.Right;
    public override Action<UIElement> _customBehaviour { get; protected set; }

    public RightArrowCommand(Action<UIElement> customBehaviour = null) => _customBehaviour = customBehaviour; 

    protected override void DefaultBehaviour(UIElement uiElement)
    {
        if (uiElement.CharIndex + 1 > uiElement.Text.Length) return;
        if (uiElement.Text[uiElement.CharIndex] == '\n') return;
        
        uiElement.SetCharIndex(uiElement.CharIndex + 1);
    }
}