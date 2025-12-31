using System;
using Microsoft.Xna.Framework.Input;

public class EnterCommand : KeyCommand
{
    public override bool RequiresControl { get; } = false;
    public override bool RequiresShift { get; } = false;
    public override bool Repeats { get; } = false;
    public override Keys AssignedKey { get; } = Keys.Enter;
    public override Action<UIElement> _customBehaviour { get; protected set; }

    public EnterCommand(Action<UIElement> customBehaviour = null) => _customBehaviour = customBehaviour; 

    protected override void DefaultBehaviour(UIElement uiElement) {}
}