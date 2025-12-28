using Microsoft.Xna.Framework.Input;

public class DownArrowCommand : IKeyCommand
{
    public bool RequiresControl { get; } = false;
    public bool Repeats { get; } = true;
    public Keys AssignedKey { get; } = Keys.Down;

    public void Execute(UIElement uiElement)
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