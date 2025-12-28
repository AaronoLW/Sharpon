using Microsoft.Xna.Framework.Input;

public interface IKeyCommand
{
    public bool RequiresControl { get; }
    public bool Repeats { get; }
    public Keys AssignedKey { get; }

    void Execute(UIElement uiElement);
}