using SDL3;

public readonly struct VimKeybind(SDL.Keycode keycode, Action<VimSet, TextContainer> action, VimMode vimMode, params SDL.Keycode[] modifiers)
{
    public readonly SDL.Keycode Keycode = keycode;
    public readonly Action<VimSet, TextContainer> Action = action;
    public readonly VimMode VimMode = vimMode;
    public readonly SDL.Keycode[] Modifiers = modifiers;
}
