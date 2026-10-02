using System.Security.Cryptography;
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
            new(SDL.Keycode.V, TextAction.Paste, true),
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
                if ((keybind.RequiresCtrl && Input.IsKeyDown(SDL.Keycode.LCtrl)) || (!keybind.RequiresCtrl && !Input.IsKeyDown(SDL.Keycode.LCtrl)))
                    StringHelper.ApplyTextAction(document, keybind.Action);
            }
        }
    }
}
