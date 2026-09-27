using SDL3;

public readonly struct Keybind(SDL.Keycode key, TextAction action, bool requiresCtrl)
{
    public readonly SDL.Keycode Key = key;
    public readonly TextAction Action = action;
    public readonly bool RequiresCtrl = requiresCtrl;
}
