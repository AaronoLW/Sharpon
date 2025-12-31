using System;
using Microsoft.Xna.Framework.Input;

public class DownArrowCommand : KeyCommand
{
    public override bool RequiresControl { get; } = false;
    public override bool RequiresShift { get; } = false;
    public override bool Repeats { get; } = true;
    public override Keys AssignedKey { get; } = Keys.Down;
    public override Action<UIElement> _customBehaviour { get; protected set; }

    public DownArrowCommand(Action<UIElement> customBehaviour = null) => _customBehaviour = customBehaviour; 

    protected override void DefaultBehaviour(UIElement uiElement)
    {
        bool seenFirstNewLine = false;
        for (int i = uiElement.CharIndex; i < uiElement.Text.Length; i++)
        {
            if (uiElement.Text[i] == '\n')
            {
                if (seenFirstNewLine)
                {
                    uiElement.SetCharIndex(i);
                    return;
                }
                else
                {
                    seenFirstNewLine = true;
                }
            }
        }

        uiElement.SetCharIndex(uiElement.Text.Length);
    }
}