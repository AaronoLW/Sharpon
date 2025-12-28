using Microsoft.Xna.Framework.Input;

public class BackspaceCommand : IKeyCommand
{
    public bool RequiresControl { get; } = false;
    public bool Repeats { get; } = false;
    public Keys AssignedKey { get; } = Keys.Back;

    public void Execute(UIElement uiElement)
    {
        if (uiElement.Text.Length <= 0) return;

        uiElement.RemoveTextAt(uiElement.CharIndex, 1);
        uiElement.SetCharIndex(uiElement.CharIndex - 1);
    }
}