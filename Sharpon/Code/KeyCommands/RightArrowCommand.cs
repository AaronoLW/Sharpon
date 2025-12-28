using Microsoft.Xna.Framework.Input;

public class RightArrowCommand : IKeyCommand
{
    public bool RequiresControl { get; } = false;
    public bool Repeats { get; } = true;
    public Keys AssignedKey { get; } = Keys.Right;

    public void Execute(UIElement uiElement)
    {
        if (uiElement.CharIndex + 1 > uiElement.Text.Length) return;
        if (uiElement.Text[uiElement.CharIndex] == '\n') return;
        
        uiElement.SetCharIndex(uiElement.CharIndex + 1);
    }
}