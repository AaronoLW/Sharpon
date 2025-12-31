using System;
using Microsoft.Xna.Framework.Input;

public class LeftArrowCommand : KeyCommand
{
    public override bool RequiresControl { get; } = false;
    public override bool RequiresShift { get; } = false;
    public override bool Repeats { get; } = true;
    public override Keys AssignedKey { get; } = Keys.Left;
    public override Action<UIElement> _customBehaviour { get; protected set; }

    public LeftArrowCommand(Action<UIElement> customBehaviour = null) => _customBehaviour = customBehaviour; 

    protected override void DefaultBehaviour(UIElement uiElement)
    {
        if (uiElement.CharIndex - 1 < 0) return;
        if (uiElement.Text[uiElement.CharIndex - 1] == '\n') return;

        uiElement.SetCharIndex(uiElement.CharIndex - 1);
    }
}