using SDL3;

public readonly struct Keybind(SDL.Keycode key, TextAction action, bool requiresCtrl, bool requiresShift, Func<Dictionary<string, string>, bool>? condition = null)
{
    public readonly SDL.Keycode Key = key;
    public readonly TextAction Action = action;
    public readonly bool RequiresCtrl = requiresCtrl;
    public readonly bool RequiresShift = requiresShift;
    public readonly Func<Dictionary<string, string>, bool>? Condition = condition;
}
