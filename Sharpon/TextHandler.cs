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

            if (InputHandler.IsKeyDown(SDL.Keycode.Backspace) && TryPress(SDL.Keycode.Backspace))
            {
                int jumpCharAmount = JumpLeft(stringBuilder, newCharIndex);
                stringBuilder.Remove(newCharIndex - jumpCharAmount, jumpCharAmount);
                newCharIndex -= jumpCharAmount;
            }

            if (InputHandler.IsKeyDown(SDL.Keycode.Left) && TryPress(SDL.Keycode.Left))
            {
                int jumpCharAmount = JumpLeft(stringBuilder, newCharIndex);
                newCharIndex -= jumpCharAmount;
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

        if (InputHandler.IsKeyDown(SDL.Keycode.Backspace))
        {
            if (TryPress(SDL.Keycode.Backspace) && stringBuilder.Length > 0)
            {
                if (newCharIndex > 0)
                {
                    if (newCharIndex < stringBuilder.Length)
                    {
                        if (stringBuilder[newCharIndex] == ')' && stringBuilder[newCharIndex - 1] == '(' ||
                            stringBuilder[newCharIndex] == '}' && stringBuilder[newCharIndex - 1] == '{' ||
                            stringBuilder[newCharIndex] == '"' && stringBuilder[newCharIndex - 1] == '"' ||
                            stringBuilder[newCharIndex] == ']' && stringBuilder[newCharIndex - 1] == '[')
                        {
                            stringBuilder.Remove(newCharIndex - 1, 2);
                            newCharIndex--;
                        }
                        else
                        {
                            stringBuilder.Remove(newCharIndex - 1, 1);
                            newCharIndex--;
                        }
                    }
                    else
                    {
                        stringBuilder.Remove(newCharIndex - 1, 1);
                        newCharIndex--;
                    }
                }
            }
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Return))
        {
            if (TryPress(SDL.Keycode.Return, REPEAT_RATE))
            {
                stringBuilder.Insert(newCharIndex, "\n");
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

        if (InputHandler.IsKeyDown(SDL.Keycode.Down) && TryPress(SDL.Keycode.Down))
        {
            newCharIndex = JumpDown(stringBuilder, newCharIndex);
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

    private static int JumpLeft(StringBuilder stringBuilder, int newCharIndex)
    {
        int charAmount = 0;
        bool haltForSpace = true;

        if (stringBuilder[newCharIndex - 1 < 0 ? 0 : newCharIndex - 1] == ' ') haltForSpace = false;

        for (int i = newCharIndex; i > -1; i--)
        {
            if (i > stringBuilder.Length - 1) continue;
            if (i == 0) { charAmount++; break; }
            
            charAmount++;

            if (stringBuilder[i - 1] == ' ' && haltForSpace) break;
            if (stringBuilder[i - 1] != ' ' && !haltForSpace) break;


            if (stringBuilder[i - 1] == '(' ||
                stringBuilder[i - 1] == '"' ||
                stringBuilder[i - 1] == '[' ||
                stringBuilder[i - 1] == '{' ||
                stringBuilder[i - 1] == '\n')
            break;
        }

        return charAmount;
    }

    private static int JumpDown(StringBuilder stringBuilder, int newCharIndex)
    {
        bool seenNewLine = false;

        for (int i = newCharIndex; i < stringBuilder.Length; i++)
        {
            if (stringBuilder[i] == '\n')
            {
                if (!seenNewLine)
                {
                    seenNewLine = true;
                }
                else
                {
                    newCharIndex = i;
                    break;
                }
            }

            if (i == stringBuilder.Length - 1)
            {
                newCharIndex = i + 1;
                break;
            }
        }

        return newCharIndex;
    }
}