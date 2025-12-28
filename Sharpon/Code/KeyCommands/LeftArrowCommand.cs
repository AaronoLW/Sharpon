using Microsoft.Xna.Framework.Input;

public class LeftArrowCommand : IKeyCommand
{
    public bool RequiresControl { get; } = false;
    public bool Repeats { get; } = true;
    public Keys AssignedKey { get; } = Keys.Left;

    public void Execute(UIElement uiElement)
    {
        if (uiElement.CharIndex - 1 < 0) return;
        if (uiElement.Text[uiElement.CharIndex - 1] == '\n') return;

        uiElement.SetCharIndex(uiElement.CharIndex - 1);
    }
}