using System;
using Microsoft.Xna.Framework.Input;

public class TabCommand : KeyCommand
{
    public override bool RequiresControl { get; } = false;
    public override bool RequiresShift { get; } = false;
    public override bool Repeats { get; } = true;
    public override Keys AssignedKey { get; } = Keys.Tab;
    public override Action<UIElement> _customBehaviour { get; protected set; }

    public TabCommand(Action<UIElement> customBehaviour = null) => _customBehaviour = customBehaviour; 

    protected override void DefaultBehaviour(UIElement uiElement)
    {
        uiElement.SetText(uiElement.Text.Insert(uiElement.CharIndex, "    "));
        uiElement.SetCharIndex(uiElement.CharIndex + 4);
    }
}