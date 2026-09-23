using SDL3;
using SmashFramework;

public class KeybindHandler
{
    public void HandleKeybinds(TextDocument document)
    {
        if (Input.IsKeyDown(SDL.Keycode.LCtrl))
        {
            if (Program.IsKeyDown(SDL.Keycode.Backspace))
            {
                document.TryRemoveBackwards(document.GetJumpBackLength());
            }

            if (Program.IsKeyDown(SDL.Keycode.Left))
            {
                document.CharIndex -= document.GetJumpBackLength();
            }

            if (Program.IsKeyDown(SDL.Keycode.Right))
            {
                document.CharIndex += document.GetJumpForwardLength();
            }
        }
        else
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

            if (Program.IsKeyDown(SDL.Keycode.Backspace))
            {
                document.TryRemoveBackwards(1);
            }

            if (Program.IsKeyDown(SDL.Keycode.Return))
            {
                document.Insert('\n');
            }

            if (Program.IsKeyDown(SDL.Keycode.Left))
            {
                if (document.CharIndex > 0)
                    document.CharIndex--;
            }

            if (Program.IsKeyDown(SDL.Keycode.Right))
            {
                if (document.CharIndex < document.Text.Length)
                    document.CharIndex++;
            }

            if (Program.IsKeyDown(SDL.Keycode.Tab))
            {
                document.Insert("    ");
            }

            if (Program.IsKeyDown(SDL.Keycode.Up))
            {
                document.MoveUp();
            }

            if (Program.IsKeyDown(SDL.Keycode.Down))
            {
                document.MoveDown();
            }

            if (Program.IsKeyDown(SDL.Keycode.End))
            {
                document.CharIndex = document.GetLineStartIndex() + document.GetLineLength();
            }

            if (Program.IsKeyDown(SDL.Keycode.Home))
            {
                document.CharIndex = document.GetLineStartIndex();
            }
        }
    }
}
