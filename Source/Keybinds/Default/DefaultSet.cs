using SDL3;
using SmashFramework;

public class DefaultSet : IKeybindSet
{
    private readonly List<Keybind> _keybinds = [
        new(SDL.Keycode.Left,  (set, document) => document.MoveLeft()),
        new(SDL.Keycode.Right, (set, document) => document.MoveRight()),
        new(SDL.Keycode.Up,    (set, document) => document.MoveUp()),
        new(SDL.Keycode.Down,  (set, document) => document.MoveDown()),

        new(SDL.Keycode.Left,  (set, document) => document.MoveLeft(document.GetJumpLeftLength()),   SDL.Keycode.LCtrl),
        new(SDL.Keycode.Right, (set, document) => document.MoveRight(document.GetJumpRightLength()), SDL.Keycode.LCtrl),

        new(SDL.Keycode.Backspace, (set, document) => document.RemoveBackwards(1)),
        new(SDL.Keycode.Backspace, (set, document) => document.RemoveBackwards(document.GetJumpLeftLength()), SDL.Keycode.LCtrl),

        new(SDL.Keycode.Return, (set, document) => document.InsertNewLine()),
        new(SDL.Keycode.Return, (set, document) => document.InsertNewLineOnPreviousLine(), SDL.Keycode.LShift),

        new(SDL.Keycode.Tab, (set, document) => document.InsertTab()),
    ];

    public void HandleKeybinds(TextDocument document)
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
                document.Insert(((char)insert).ToString());
                document.MoveLeft();
            }
        }

        SDL.Keycode[] pressedModifiers = KeybindHelper.GetPressedModifiers();

        foreach (Keybind keybind in _keybinds)
        {
            if (keybind.Modifiers.Length != pressedModifiers.Length)
                continue;

            if (keybind.Repeats)
            {
                if (Program.IsKeyDown(keybind.Keycode))
                {
                    keybind.Action.Invoke(this, document);
                }
            }
            else
            {
                if (Input.IsKeyPressed(keybind.Keycode))
                {
                    keybind.Action.Invoke(this, document);
                }
            }
        }
    }
}
