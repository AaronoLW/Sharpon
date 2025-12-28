using Microsoft.Xna.Framework.Input;

public class NewLineCommand : IKeyCommand
{
    public bool RequiresControl { get; } = false;
    public bool Repeats { get; } = true;
    public Keys AssignedKey { get; } = Keys.Enter;
    
    public void Execute(UIElement uiElement)
    {
        uiElement.InsertTextAt(uiElement.CharIndex, "\n");
        uiElement.SetCharIndex(uiElement.CharIndex + 1);
    }
}