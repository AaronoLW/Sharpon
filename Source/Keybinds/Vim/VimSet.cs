using SDL3;
using SmashFramework;

public class VimSet : IKeybindSet
{
    private VimMode _vimMode = VimMode.Insert;
    private readonly List<VimKeybind> _keybinds = [
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
        new(SDL.Keycode.Left,         (vimSet, document) => document.MoveLeft(),              VimMode.Normal),
        new(SDL.Keycode.Up,         (vimSet, document) => document.MoveDown(),                VimMode.Normal),
        new(SDL.Keycode.Down,         (vimSet, document) => document.MoveUp(),                VimMode.Normal),
        new(SDL.Keycode.Right,         (vimSet, document) => document.MoveRight(),            VimMode.Normal),

        new(SDL.Keycode.Backspace, (vimSet, document) => document.RemoveBackwards(1),                                              VimMode.Insert),
        new(SDL.Keycode.Backspace, (vimSet, document) => document.RemoveBackwards(document.GetJumpLeftLength()),                   VimMode.Insert, SDL.Keycode.LCtrl),
        new(SDL.Keycode.Escape,    (vimSet, document) => { document.MoveLeft(); vimSet.SetVimMode(VimMode.Normal, document); },    VimMode.Insert),
        new(SDL.Keycode.Return,    (vimSet, document) => document.InsertNewLine(),                                                 VimMode.Insert),
        new(SDL.Keycode.Tab,       (vimSet, document) => document.InsertTab(),                                                     VimMode.Insert),
        new(SDL.Keycode.Left,         (vimSet, document) => document.MoveLeft(),                 VimMode.Insert),
        new(SDL.Keycode.Up,         (vimSet, document) => document.MoveDown(),                   VimMode.Insert),
        new(SDL.Keycode.Down,         (vimSet, document) => document.MoveUp(),                   VimMode.Insert),
        new(SDL.Keycode.Right,         (vimSet, document) => document.MoveRight(),               VimMode.Insert),
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

        SDL.Keycode[] pressedModifiers = GetPressedModifiers();

        foreach (VimKeybind keybind in _keybinds)
        {
            if (_vimMode != keybind.VimMode)
                continue;

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

    private void SetVimMode(VimMode vimMode, TextDocument document)
    {
        _vimMode = vimMode;

        if (_vimMode == VimMode.Insert)
            document.CaretStyle = CaretStyle.Beam;

        if (_vimMode == VimMode.Normal)
            document.CaretStyle = CaretStyle.Block;
    }

    private SDL.Keycode[] GetPressedModifiers()
    {
        List<SDL.Keycode> modifiers = [];

        if (Input.IsKeyDown(SDL.Keycode.LShift))
            modifiers.Add(SDL.Keycode.LShift);

        if (Input.IsKeyDown(SDL.Keycode.LCtrl))
            modifiers.Add(SDL.Keycode.LCtrl);

        return [.. modifiers];
    }
}
