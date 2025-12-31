using System;
using Microsoft.Xna.Framework.Input;

public class NewLineCommand : KeyCommand
{
    public override bool RequiresControl { get; } = false;
    public override bool RequiresShift { get; } = false;
    public override bool Repeats { get; } = true;
    public override Keys AssignedKey { get; } = Keys.Enter;
    public override Action<UIElement> _customBehaviour { get; protected set; }

    public NewLineCommand(Action<UIElement> customBehaviour = null) => _customBehaviour = customBehaviour; 
    
    protected override void DefaultBehaviour(UIElement uiElement)
    {
        uiElement.InsertTextAt(uiElement.CharIndex, "\n");
        uiElement.SetCharIndex(uiElement.CharIndex + 1);
    }
}