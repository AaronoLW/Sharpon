using SDL3;

public readonly struct Keybind(SDL.Keycode keycode, Action<DefaultSet, ITextContainer> action, params SDL.Keycode[] modifiers)
{
    public readonly SDL.Keycode Keycode = keycode;
    public readonly Action<DefaultSet, ITextContainer> Action = action;
    public readonly bool Repeats = true;
    public readonly SDL.Keycode[] Modifiers = modifiers;
}
