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

        if (InputHandler.IsKeyDown(SDL.Keycode.LCtrl))
        {
            TextInputOperation? textInputOperation = null;

            if (InputHandler.IsKeyDown(SDL.Keycode.Plus) && TryPress(SDL.Keycode.Plus))
            {
                textInputOperation = TextInputOperation.IncreasePointSize;
            }

            if (InputHandler.IsKeyDown(SDL.Keycode.Minus) && TryPress(SDL.Keycode.Minus))
            {
                textInputOperation = TextInputOperation.DecreasePointSize;
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
            stringBuilder.Append(InputHandler.TextInput);
            newCharIndex += InputHandler.TextInput.Length;
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Backspace))
        {
            if (TryPress(SDL.Keycode.Backspace) && stringBuilder.Length > 0)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
                newCharIndex--;
            }
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Return))
        {
            if (TryPress(SDL.Keycode.Return, REPEAT_RATE))
            {
                stringBuilder.Append("\n");
                newCharIndex++;
            }
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Right))
        {
            if (TryPress(SDL.Keycode.Right))
            {
                if (newCharIndex + 1 <= stringBuilder.Length)
                {
                    newCharIndex++;
                }
            }
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Left))
        {
            if (TryPress(SDL.Keycode.Left))
            {
                if (newCharIndex > 0)
                {
                    newCharIndex--;
                }
            }
        }

        return new TextInputInfo()
        {
            NewText = stringBuilder.ToString(),
            NewCharIndex = newCharIndex
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