using System;
using Microsoft.Xna.Framework.Input;

public class EscapeCommand : KeyCommand
{
    public override bool RequiresControl { get; } = false;
    public override bool RequiresShift { get; } = false;
    public override bool Repeats { get; } = true;
    public override Keys AssignedKey { get; } = Keys.Escape;
    public override Action<UIElement> _customBehaviour { get; protected set; }

    public EscapeCommand(Action<UIElement> customBehaviour = null) => _customBehaviour = customBehaviour; 

    protected override void DefaultBehaviour(UIElement uiElement) {}
}