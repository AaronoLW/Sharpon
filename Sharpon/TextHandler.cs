using System.Reflection.PortableExecutable;
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

            if (InputHandler.IsKeyDown(SDL.Keycode.Right) && TryPress(SDL.Keycode.Right))
            {
                int jumpCharAmount = JumpRight(stringBuilder, newCharIndex);
                newCharIndex += jumpCharAmount;
            }

            if (InputHandler.IsKeyDown(SDL.Keycode.Return) && TryPress(SDL.Keycode.Return))
            {
                int endOfLine = GetEndOfCurrentLine(stringBuilder, newCharIndex);
                int firstLetter = GetFirstLetterOnLine(stringBuilder, newCharIndex);

                stringBuilder.Insert(endOfLine, '\n');
                newCharIndex = endOfLine + 1;

                for (int i = 0; i < firstLetter; i++)
                {
                    stringBuilder.Insert(newCharIndex, " ");
                    newCharIndex++;
                }
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

        if (InputHandler.IsKeyDown(SDL.Keycode.Return) && TryPress(SDL.Keycode.Return, REPEAT_RATE))
        {
            int firstLetter = GetFirstLetterOnLine(stringBuilder, newCharIndex);
            stringBuilder.Insert(newCharIndex, "\n");
            newCharIndex++;

            for (int i = 0; i < firstLetter; i++)
            {
                stringBuilder.Insert(newCharIndex, " ");
                newCharIndex++;
            }

        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Tab) && TryPress(SDL.Keycode.Tab))
        {
            stringBuilder.Insert(newCharIndex, "    ");
            newCharIndex += 4;
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Right) && TryPress(SDL.Keycode.Right))
        {
            if (newCharIndex + 1 <= stringBuilder.Length)
            {
                newCharIndex++;
            }
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Left) && TryPress(SDL.Keycode.Left))
        {
            if (newCharIndex > 0)
            {
                newCharIndex--;
            }
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Down) && TryPress(SDL.Keycode.Down))
        {
            newCharIndex = JumpDown(stringBuilder, newCharIndex);
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Up) && TryPress(SDL.Keycode.Up))
        {
            newCharIndex = JumpUp(stringBuilder, newCharIndex);
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

        if (newCharIndex == 0) return 0;
        if (stringBuilder.Length == 0) return 0;
        if (stringBuilder[newCharIndex - 1 < 0 ? 0 : newCharIndex - 1] == ' ') haltForSpace = false;

        for (int i = newCharIndex; i > -1; i--)
        {
            if (i > stringBuilder.Length - 1) continue;
            if (i == 0) { charAmount = newCharIndex; break; }
            
            charAmount++;

            if (stringBuilder[i - 1] == '\n' && i != newCharIndex) { charAmount--; break; }

            if (stringBuilder[i - 1] == ' ' && haltForSpace) break;
            if (stringBuilder[i - 1] != ' ' && !haltForSpace) break;

            if (stringBuilder[i - 1] == '(' ||
                stringBuilder[i - 1] == '"' ||
                stringBuilder[i - 1] == '[' ||
                stringBuilder[i - 1] == '{' ||
                stringBuilder[i - 1] == '.' ||
                stringBuilder[i - 1] == ',' )
            break;

        }

        return charAmount;
    }

    private static int JumpRight(StringBuilder stringBuilder, int newCharIndex)
    {
        int charAmount = 0;
        bool haltForSpace = true;

        if (stringBuilder.Length == 0) return 0;
        if (newCharIndex == stringBuilder.Length) return 0;
        if (stringBuilder[newCharIndex + 1] == ' ') haltForSpace = false;

        if (stringBuilder[newCharIndex] == ' ') { charAmount++; newCharIndex++; }
        if (stringBuilder[newCharIndex] == '\n') { return 1; }

        for (int i = newCharIndex; i < stringBuilder.Length; i++)
        {
            if (stringBuilder[i] == ' ' && haltForSpace) break;
            if (stringBuilder[i] != ' ' && !haltForSpace) break;

            if (stringBuilder[i] == ')' ||
                stringBuilder[i] == '"' ||
                stringBuilder[i] == ']' ||
                stringBuilder[i] == '}' ||
                stringBuilder[i] == '.' ||
                stringBuilder[i] == ',' ||
                stringBuilder[i] == '\n' )
            break;

            charAmount++;
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

    private static int JumpUp(StringBuilder stringBuilder, int newCharIndex)
    {
        for (int i = newCharIndex - 1; i > 0; i--)
        {
            if (stringBuilder[i] == '\n')
            {
                return i;
            }
        }

        return 0;
    }

    private static int GetEndOfCurrentLine(StringBuilder stringBuilder, int newCharIndex)
    {
        for (int i = newCharIndex; i < stringBuilder.Length; i++)
        {
            if (stringBuilder[i] == '\n') return i;
        }

        return stringBuilder.Length;
    }

    private static int GetFirstLetterOnLine(StringBuilder stringBuilder, int newCharIndex)
    {
        int amount = 0;

        for (int i = newCharIndex; i > -1; i--)
        {
            if (i == stringBuilder.Length) continue;
            if (stringBuilder[i] == '\n') return amount;
            
            if (stringBuilder[i] != ' ')
            {
                amount = 0;
            }
            else
            {
                amount++;
            }
        }

        return amount;
    }
}