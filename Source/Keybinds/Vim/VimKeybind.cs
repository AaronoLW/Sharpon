using SDL3;

public readonly struct VimKeybind(SDL.Keycode keycode, Action<VimSet, TextDocument> action, VimMode vimMode, params SDL.Keycode[] modifiers)
{
    public readonly SDL.Keycode Keycode = keycode;
    public readonly Action<VimSet, TextDocument> Action = action;
    public readonly bool Repeats = true;
    public readonly VimMode VimMode = vimMode;
    public readonly SDL.Keycode[] Modifiers = modifiers;
}
