public readonly struct KeybindSet(Dictionary<string, string> variables, Func<Dictionary<string, string>, bool> textInputCondition, List<Keybind> keybinds)
{
    public readonly Dictionary<string, string> Variables = variables;
    public readonly Func<Dictionary<string, string>, bool> TextInputCondition = textInputCondition;
    public readonly List<Keybind> Keybinds = keybinds;
}
