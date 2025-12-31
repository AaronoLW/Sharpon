using System;
using Microsoft.Xna.Framework.Input;

public class BackspaceCommand : KeyCommand
{
    public override bool RequiresControl { get; } = false;
    public override bool RequiresShift { get; } = false;
    public override bool Repeats { get; } = true;
    public override Keys AssignedKey { get; } = Keys.Back;
    public override Action<UIElement> _customBehaviour { get; protected set; }

    public BackspaceCommand(Action<UIElement> customBehaviour = null) => _customBehaviour = customBehaviour; 

    protected override void DefaultBehaviour(UIElement uiElement)
    {
        if (uiElement.Text.Length <= 0) return;
        if (uiElement.CharIndex == 0) return;

        uiElement.RemoveTextAt(uiElement.CharIndex, 1);
        uiElement.SetCharIndex(uiElement.CharIndex - 1);
    }
}