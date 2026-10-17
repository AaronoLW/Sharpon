using SDL3;
using SmashFramework;

public class VimSet : IKeybindSet
{
    private VimMode _vimMode = VimMode.Insert;
    private readonly List<VimKeybind> _keybinds = [
        // Normal mode
        new(SDL.Keycode.H,         (vimSet, document) => document.MoveLeft(),                 VimMode.Normal),
        new(SDL.Keycode.J,         (vimSet, document) => document.MoveDown(),                 VimMode.Normal),
        new(SDL.Keycode.K,         (vimSet, document) => document.MoveUp(),                   VimMode.Normal),
        new(SDL.Keycode.L,         (vimSet, document) => document.MoveRight(),                VimMode.Normal),
        new(SDL.Keycode.I,         (vimSet, document) => vimSet.SetVimMode(VimMode.Insert, document),    VimMode.Normal),
        new(SDL.Keycode.I,         (vimSet, document) => vimSet.SetVimMode(VimMode.Insert, document),    VimMode.Normal),
        new(SDL.Keycode.A,         (vimSet, document) => { vimSet.SetVimMode(VimMode.Insert, document); document.MoveRight(); },    VimMode.Normal),
        new(SDL.Keycode.I,         (vimSet, document) => { vimSet.SetVimMode(VimMode.Insert, document); document.JumpToStartOfLine(); },    VimMode.Normal, SDL.Keycode.LShift),
        new(SDL.Keycode.A,         (vimSet, document) => { vimSet.SetVimMode(VimMode.Insert, document); document.JumpToEndOfLine(); },      VimMode.Normal, SDL.Keycode.LShift),
        new(SDL.Keycode.E,         (vimSet, document) => document.MoveRight(document.GetJumpRightLength()),  VimMode.Normal),
        new(SDL.Keycode.B,         (vimSet, document) => document.MoveLeft(document.GetJumpLeftLength()),   VimMode.Normal),
        new(SDL.Keycode.O,         (vimSet, document) => { vimSet.SetVimMode(VimMode.Insert, document); document.InsertNewLine(); },   VimMode.Normal),
        new(SDL.Keycode.O,         (vimSet, document) => { vimSet.SetVimMode(VimMode.Insert, document); document.InsertNewLineOnPreviousLine(); },   VimMode.Normal, SDL.Keycode.LShift),
        new(SDL.Keycode.P,         (vimSet, document) => document.PasteClipboard(),           VimMode.Normal),
        new(SDL.Keycode.Left,      (vimSet, document) => document.MoveLeft(),              VimMode.Normal),
        new(SDL.Keycode.Down,        (vimSet, document) => document.MoveDown(),                VimMode.Normal),
        new(SDL.Keycode.Up,      (vimSet, document) => document.MoveUp(),                VimMode.Normal),
        new(SDL.Keycode.Right,     (vimSet, document) => document.MoveRight(),            VimMode.Normal),
        new(SDL.Keycode.V,     (vimSet, document) => { document.StartSelection(); vimSet.SetVimMode(VimMode.Visual, document); },            VimMode.Normal),
        new(SDL.Keycode.V,     (vimSet, document) => { document.StartSelection(); vimSet.SetVimMode(VimMode.Visual, document); },            VimMode.Normal, SDL.Keycode.LShift),

        // Insert mode
        new(SDL.Keycode.Backspace, (vimSet, document) => document.RemoveBackwards(1),                                              VimMode.Insert),
        new(SDL.Keycode.Backspace, (vimSet, document) => document.RemoveBackwards(document.GetJumpLeftLength()),                   VimMode.Insert, SDL.Keycode.LCtrl),
        new(SDL.Keycode.Escape,    (vimSet, document) => { document.MoveLeft(); vimSet.SetVimMode(VimMode.Normal, document); },    VimMode.Insert),
        new(SDL.Keycode.Return,    (vimSet, document) => document.InsertNewLine(),                                                 VimMode.Insert),
        new(SDL.Keycode.Tab,       (vimSet, document) => document.InsertTab(),                                                     VimMode.Insert),
        new(SDL.Keycode.Left,      (vimSet, document) => document.MoveLeft(),                 VimMode.Insert),
        new(SDL.Keycode.Down,        (vimSet, document) => document.MoveDown(),                   VimMode.Insert),
        new(SDL.Keycode.Up,      (vimSet, document) => document.MoveUp(),                   VimMode.Insert),
        new(SDL.Keycode.Right,     (vimSet, document) => document.MoveRight(),               VimMode.Insert),

        // Delete mode
        new(SDL.Keycode.D,     (vimSet, document) => vimSet.SetVimMode(VimMode.Delete, document),               VimMode.Normal),
        new(SDL.Keycode.D,     (vimSet, document) => { document.DeleteLine(); vimSet.SetVimMode(VimMode.Normal, document); },               VimMode.Delete),
        new(SDL.Keycode.Escape,     (vimSet, document) => vimSet.SetVimMode(VimMode.Normal, document),               VimMode.Delete),

        // Visual mode
        new(SDL.Keycode.Escape,     (vimSet, document) => { vimSet.SetVimMode(VimMode.Normal, document); document.EndSelection(); },               VimMode.Visual),
        new(SDL.Keycode.H,         (vimSet, document) => document.MoveLeft(),                 VimMode.Visual),
        new(SDL.Keycode.J,         (vimSet, document) => document.MoveDown(),                 VimMode.Visual),
        new(SDL.Keycode.K,         (vimSet, document) => document.MoveUp(),                   VimMode.Visual),
        new(SDL.Keycode.L,         (vimSet, document) => document.MoveRight(),                VimMode.Visual),
        new(SDL.Keycode.D,         (vimSet, document) => { document.DeleteSelection(); document.EndSelection(); vimSet.SetVimMode(VimMode.Normal, document); },                VimMode.Visual),
        new(SDL.Keycode.E,         (vimSet, document) => document.MoveRight(document.GetJumpRightLength()),  VimMode.Visual),
        new(SDL.Keycode.B,         (vimSet, document) => document.MoveLeft(document.GetJumpLeftLength()),   VimMode.Visual),
    ];

    public void HandleKeybinds(TextDocument document)
    {
        if (_vimMode == VimMode.Insert)
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
        }

        SDL.Keycode[] pressedModifiers = KeybindHelper.GetPressedModifiers();

        foreach (VimKeybind keybind in _keybinds)
        {
            if (_vimMode != keybind.VimMode)
                continue;

            if (!pressedModifiers.All(p => keybind.Modifiers.Contains(p)))
                continue;

            if (keybind.Repeats)
            {
                if (Program.IsKeyDown(keybind.Keycode))
                {
                    keybind.Action.Invoke(this, document);
                    break;
                }
            }
            else
            {
                if (Input.IsKeyPressed(keybind.Keycode))
                {
                    keybind.Action.Invoke(this, document);
                    break;
                }
            }
        }
    }

    private void SetVimMode(VimMode vimMode, TextDocument document)
    {
        _vimMode = vimMode;

        if (_vimMode == VimMode.Insert)
            document.CaretStyle = CaretStyle.Beam;

        if (_vimMode == VimMode.Delete)
            document.CaretStyle = CaretStyle.Underline;

        if (_vimMode == VimMode.Normal)
            document.CaretStyle = CaretStyle.Block;

        if (_vimMode == VimMode.Visual)
            document.CaretStyle = CaretStyle.Block;
    }
}
