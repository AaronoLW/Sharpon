using SDL3;
using SmashFramework;

public class KeybindHandler
{
    private List<Keybind> _keybinds = [];

    public void Register(Keybind keybind)
    {
        _keybinds.Add(keybind);
    }

    public void HandleKeybinds(TextDocument document)
    {
        foreach (Keybind keybind in _keybinds)
        {
            if (Program.IsKeyDown(keybind.Key))
            {
                if ((keybind.RequiresCtrl && Input.IsKeyDown(SDL.Keycode.LCtrl)) || !keybind.RequiresCtrl)
                    ApplyTextAction(document, keybind.Action);
            }
        }
    }

    private void ApplyTextAction(TextDocument document, TextAction action)
    {
        switch (action)
        {
            case TextAction.DeleteCharacter:
                document.TryRemoveBackwards(1);
                break;

            case TextAction.DeleteWord:
                document.TryRemoveBackwards(document.GetJumpBackLength());
                break;

            case TextAction.MoveLeft:
                if (document.CharIndex > 0)
                    document.CharIndex--;
                break;

            case TextAction.MoveRight:
                if (document.CharIndex < document.Text.Length)
                    document.CharIndex++;
                break;

            case TextAction.JumpLeft:
                document.CharIndex -= document.GetJumpBackLength();
                break;

            case TextAction.JumpRight:
                document.CharIndex += document.GetJumpForwardLength();
                break;

            case TextAction.MoveUp:
                document.MoveUp();
                break;

            case TextAction.MoveDown:
                document.MoveDown();
                break;

            case TextAction.InsertNewline:
                document.Insert('\n');
                break;

            case TextAction.InsertTab:
                document.Insert("    ");
                break;

            case TextAction.GoToStartOfLine:
                document.CharIndex = document.GetLineStartIndex();
                break;

            case TextAction.GoToEndOfLine:
                document.CharIndex = document.GetLineStartIndex() + document.GetLineLength();
                break;

            case TextAction.DeleteCharacterForward:
                if (document.CharIndex != document.Text.Length)
                {
                    document.CharIndex++;
                    document.TryRemoveBackwards(1);
                }
                break;

            case TextAction.DeleteWordForward:
                int length = document.GetJumpForwardLength();
                document.CharIndex += length;
                document.TryRemoveBackwards(length);
                break;
        }
    }
}
