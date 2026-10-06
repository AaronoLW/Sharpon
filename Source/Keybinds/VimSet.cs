using SDL3;
using SmashFramework;

public class VimSet : IKeybindSet
{
    private VimMode _vimMode = VimMode.Insert;

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

            if (Program.IsKeyDown(SDL.Keycode.Backspace))
                document.RemoveBackwards(1);
        }
    }
}
