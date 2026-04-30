using System.Text;
using SDL3;
using Smash.Input;

public static class TextHandler
{
    private const float REPEAT_RATE = 0.2f;
    private const float FAST_REPEAT_RATE = 0.03f;

    private static Dictionary<SDL.Keycode, float> _repeatDelay = new();

    public static TextInputInfo Handle(string initialText, int charIndex)
    {
        StringBuilder stringBuilder = new(initialText);
        int newCharIndex = charIndex;

        TextInputOperation? textInputOperation = null;

        if (InputHandler.IsKeyDown(SDL.Keycode.LCtrl))
        {

            if (InputHandler.IsKeyDown(SDL.Keycode.Plus) && TryPress(SDL.Keycode.Plus))
                textInputOperation = TextInputOperation.IncreasePointSize;

            if (InputHandler.IsKeyDown(SDL.Keycode.Minus) && TryPress(SDL.Keycode.Minus))
                textInputOperation = TextInputOperation.DecreasePointSize;

            if (InputHandler.IsKeyDown(SDL.Keycode.Backspace) && TryPress(SDL.Keycode.Backspace))
                textInputOperation = TextInputOperation.DeleteWord;

            if (InputHandler.IsKeyDown(SDL.Keycode.Left) && TryPress(SDL.Keycode.Left))
                textInputOperation = TextInputOperation.JumpLeft;

            if (InputHandler.IsKeyDown(SDL.Keycode.Right) && TryPress(SDL.Keycode.Right))
                textInputOperation = TextInputOperation.JumpRight;

            if (InputHandler.IsKeyDown(SDL.Keycode.Return) && TryPress(SDL.Keycode.Return))
            {
                //int endOfLine = GetEndOfCurrentLine(stringBuilder, newCharIndex);
                //int firstLetter = GetFirstLetterOnLine(stringBuilder, newCharIndex);

                //stringBuilder.Insert(endOfLine, '\n');
                //newCharIndex = endOfLine + 1;

                //for (int i = 0; i < firstLetter; i++)
                //{
                //    stringBuilder.Insert(newCharIndex, " ");
                //    newCharIndex++;
                //}
            }

            if (InputHandler.IsKeyPressed(SDL.Keycode.S))
            {
                textInputOperation = TextInputOperation.SaveFile;
            } 

            if (InputHandler.IsKeyPressed(SDL.Keycode.P))
            {
                textInputOperation = TextInputOperation.ToggleFileDialog;
            }

            if (InputHandler.IsKeyPressed(SDL.Keycode.V))
            {
                textInputOperation = TextInputOperation.PasteClipboard;
            }

            return new TextInputInfo()
            {
                NewText = stringBuilder.ToString(),
                NewCharIndex = newCharIndex,
                TextInputOperation = textInputOperation
            };
        }

        if (InputHandler.TextInput != null)
        {
            stringBuilder.Insert(newCharIndex, InputHandler.TextInput);
            newCharIndex += InputHandler.TextInput.Length;

            if (InputHandler.TextInput == "(")
            {
                stringBuilder.Insert(newCharIndex, ")");
            }

            if (InputHandler.TextInput == "{")
            {
                stringBuilder.Insert(newCharIndex, "}");
            }

            if (InputHandler.TextInput == "[")
            {
                stringBuilder.Insert(newCharIndex, "]");
            }

            if (InputHandler.TextInput.ToCharArray().First() == '"')
            {
                stringBuilder.Insert(newCharIndex, '"');
            }
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Backspace) && TryPress(SDL.Keycode.Backspace) && stringBuilder.Length > 0)
        {
            textInputOperation = TextInputOperation.DeleteCharacter;
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Return) && TryPress(SDL.Keycode.Return, REPEAT_RATE))
        {
            textInputOperation = TextInputOperation.NewLine;
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Tab) && TryPress(SDL.Keycode.Tab))
        {
            textInputOperation = TextInputOperation.Tab;
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Right) && TryPress(SDL.Keycode.Right))
        {
            textInputOperation = TextInputOperation.MoveRight;
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Left) && TryPress(SDL.Keycode.Left))
        {
            textInputOperation = TextInputOperation.MoveLeft;
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Down) && TryPress(SDL.Keycode.Down))
        {
            textInputOperation = TextInputOperation.JumpDown;
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Up) && TryPress(SDL.Keycode.Up))
        {
            textInputOperation = TextInputOperation.JumpUp;
        }

        return new TextInputInfo()
        {
            NewText = stringBuilder.ToString(),
            NewCharIndex = newCharIndex,
            TextInputOperation = textInputOperation
        };
    }

    public static void Update(double deltaTime)
    {
        foreach (SDL.Keycode keycode in _repeatDelay.Keys)
        {
            if (!InputHandler.IsKeyDown(keycode))
            {
                _repeatDelay.Remove(keycode);
            }
            else
            {
                _repeatDelay[keycode] -= (float)deltaTime;
            }
        }
    }

    private static bool TryPress(SDL.Keycode keycode, float fastRepeatRate = FAST_REPEAT_RATE)
    {
        if (_repeatDelay.TryGetValue(keycode, out float delay))
        {
            if (delay <= 0)
            {
                _repeatDelay[keycode] = fastRepeatRate;
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            _repeatDelay.Add(keycode, REPEAT_RATE);
            return true;
        }
    }
}