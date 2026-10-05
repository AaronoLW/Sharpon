using SDL3;
using SmashFramework;

public static class KeybindHandler
{
    private static readonly KeybindSet _keybindSet = new(
        new()
        {
            { "Mode", "Normal" }
        },
        variables =>
        {
            return variables["Mode"] == "Insert";
        },
        new()
        {
            new(SDL.Keycode.H, TextAction.MoveLeft, false, false, variables => { return variables["Mode"] == "Normal"; } ),
            new(SDL.Keycode.L, TextAction.MoveRight, false, false, variables => { return variables["Mode"] == "Normal"; } ),
            new(SDL.Keycode.K, TextAction.MoveUp, false, false, variables => { return variables["Mode"] == "Normal"; } ),
            new(SDL.Keycode.J, TextAction.MoveDown, false, false, variables => { return variables["Mode"] == "Normal"; } ),
            new(SDL.Keycode.I, TextAction.None, false, false, variables => { if (variables["Mode"] == "Normal") variables["Mode"] = "Insert";  return false; } ),

            new(SDL.Keycode.Escape, TextAction.None, false, false, variables => { variables["Mode"] = "Normal";  return false; } ),
        }
    );

    public static void HandleKeybinds(TextDocument document)
    {
        if (_keybindSet.TextInputCondition.Invoke(_keybindSet.Variables))
        {
            if (Program.TextInput != null)
            {
                document.Insert(Program.TextInput);

                char? insert = null;
                if (Program.TextInput == "{") insert = '}';
                if (Program.TextInput == "(") insert = ')';
                if (Program.TextInput == "\"") insert = '"';
                if (Program.TextInput == "[") insert = ']';

                if (insert != null)
                {
                    document.Insert((char)insert);
                    document.CharIndex--;

                }
            }
        }

        foreach (Keybind keybind in _keybindSet.Keybinds)
        {
            if (Program.IsKeyDown(keybind.Key))
            {
                if (keybind.Condition == null || keybind.Condition.Invoke(_keybindSet.Variables))
                {
                    StringHelper.ApplyTextAction(document, keybind.Action);
                }
            }
        }
    }
}
