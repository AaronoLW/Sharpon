using SDL3;
using SmashFramework;

public class KeybindHandler
{
    private readonly Dictionary<string, List<Keybind>> _keybindSets = new() {
        { "Default", new() {
            new(SDL.Keycode.Left, TextAction.MoveLeft, false),
            new(SDL.Keycode.Right, TextAction.MoveRight, false),
            new(SDL.Keycode.Up, TextAction.MoveUp, false),
            new(SDL.Keycode.Down, TextAction.MoveDown, false),
            new(SDL.Keycode.Left, TextAction.JumpLeft, true),
            new(SDL.Keycode.Right, TextAction.JumpRight, true),
            new(SDL.Keycode.Backspace, TextAction.DeleteCharacter, false),
            new(SDL.Keycode.Backspace, TextAction.DeleteWord, true),
            new(SDL.Keycode.Return, TextAction.InsertNewline, false),
            new(SDL.Keycode.Tab, TextAction.InsertTab, false),
            new(SDL.Keycode.End, TextAction.GoToEndOfLine, false),
            new(SDL.Keycode.Home, TextAction.GoToStartOfLine, false),
            new(SDL.Keycode.Delete, TextAction.DeleteCharacterForward, false),
            new(SDL.Keycode.Delete, TextAction.DeleteWordForward, true),
        } },
    };

    public List<string> RegisteredKeybindSets => [.. _keybindSets.Keys];

    public string SelectedKeybindSet = "Default";

    public void RegisterSet(string name, List<Keybind> keybinds)
    {
        _keybindSets.Add(name, keybinds);
    }

    public void HandleKeybinds(TextDocument document)
    {
        if (SelectedKeybindSet == null)
            return;

        foreach (Keybind keybind in _keybindSets[SelectedKeybindSet])
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
