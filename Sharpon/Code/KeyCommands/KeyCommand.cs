using System;
using Microsoft.Xna.Framework.Input;

public abstract class KeyCommand
{
    public abstract bool RequiresControl { get; }
    public abstract bool RequiresShift { get; }
    public abstract bool Repeats { get; }
    public abstract Keys AssignedKey { get; }
    public abstract Action<UIElement> _customBehaviour { get; protected set; }

    public KeyCommand(Action<UIElement> customBehaviour = null) => _customBehaviour = customBehaviour; 

    public void Execute(UIElement uiElement)
    {
        if (_customBehaviour == null)
        {
            DefaultBehaviour(uiElement);
        }
        else
        {
            _customBehaviour.Invoke(uiElement);
        }
    }

    protected abstract void DefaultBehaviour(UIElement uiElement);
}