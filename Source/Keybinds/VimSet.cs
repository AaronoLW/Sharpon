using SDL3;
using SmashFramework;

public class VimSet : IKeybindSet
{
    private VimMode _vimMode = VimMode.Normal;

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
                    document.Insert((char)insert);
                    document.CharIndex--;
                }
            }
        }

        TextAction textAction = TextAction.None;

        if (_vimMode == VimMode.Normal)
        {
            if (Input.IsKeyDown(SDL.Keycode.LShift))
            {
                if (Input.IsKeyDown(SDL.Keycode.A))
                {
                    SetVimMode(document, VimMode.Insert);
                    textAction = TextAction.GoToEndOfLine;
                }

                if (Program.IsKeyDown(SDL.Keycode.I))
                {
                    SetVimMode(document, VimMode.Insert);
                    textAction = TextAction.GoToStartOfLine;
                }
            }
            else
            {
                if (Program.IsKeyDown(SDL.Keycode.L))
                    textAction = TextAction.MoveRight;

                if (Program.IsKeyDown(SDL.Keycode.H))
                    textAction = TextAction.MoveLeft;

                if (Program.IsKeyDown(SDL.Keycode.J))
                    textAction = TextAction.MoveDown;

                if (Program.IsKeyDown(SDL.Keycode.K))
                    textAction = TextAction.MoveUp;

                if (Program.IsKeyDown(SDL.Keycode.I))
                    SetVimMode(document, VimMode.Insert);

                if (Program.IsKeyDown(SDL.Keycode.A))
                {
                    document.CaretStyle = CaretStyle.Beam;
                    textAction = TextAction.MoveRight;
                    SetVimMode(document, VimMode.Insert);
                }

                if (Program.IsKeyDown(SDL.Keycode.B))
                    textAction = TextAction.JumpLeft;

                if (Program.IsKeyDown(SDL.Keycode.E) || Program.IsKeyDown(SDL.Keycode.W))
                    textAction = TextAction.JumpRight;

                if (Program.IsKeyDown(SDL.Keycode.P))
                    textAction = TextAction.Paste;
            }
        }

        if (_vimMode == VimMode.Insert)
        {
            if (Input.IsKeyPressed(SDL.Keycode.Escape))
            {
                document.CaretStyle = CaretStyle.Block;
                textAction = TextAction.MoveLeft;
                SetVimMode(document, VimMode.Normal);
            }

            if (Input.IsKeyDown(SDL.Keycode.LCtrl))
            {
                if (Program.IsKeyDown(SDL.Keycode.Backspace))
                    textAction = TextAction.DeleteWord;
            }
            else
            {
                if (Program.IsKeyDown(SDL.Keycode.Backspace))
                    textAction = TextAction.DeleteCharacter;

                if (Program.IsKeyDown(SDL.Keycode.Return))
                    textAction = TextAction.InsertNewline;

                if (Program.IsKeyDown(SDL.Keycode.Tab))
                    textAction = TextAction.InsertTab;
            }

        }

        document.ApplyTextAction(textAction);
    }

    private void SetVimMode(TextDocument document, VimMode vimMode)
    {
        _vimMode = vimMode;
        switch (vimMode)
        {
            case VimMode.Insert:
                document.CaretStyle = CaretStyle.Beam;
                break;

            case VimMode.Normal:
                document.CaretStyle = CaretStyle.Block;
                break;
        }
    }
}
