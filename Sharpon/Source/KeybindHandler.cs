using SDL3;
using Smash.Input;

public static class KeybindHandler
{
    private static Dictionary<SDL.Keycode, float> _keyCooldowns = new();
    private static Dictionary<SDL.Keycode, bool> _markedForFastCooldown = new();

    private static readonly float _baseCooldown = 0.11f;
    private static readonly float _baseFastCooldown = 0.03f;

    public static KeybindHandlerInfo HandleKeybinds(string initialText, int charIndex)
    {
        bool ctrlPressed = InputHandler.IsKeyDown(SDL.Keycode.LCtrl);
        bool enterPressed = InputHandler.IsKeyDown(SDL.Keycode.Return);
        bool backspacePressed = InputHandler.IsKeyDown(SDL.Keycode.Backspace);
        bool tabPressed = InputHandler.IsKeyDown(SDL.Keycode.Tab);

        string newText = initialText;
        int newCharIndex = charIndex;

        EditorCommand? editorCommand = null;
        bool mayAddTextInput = true;

        if (InputHandler.IsKeyPressed(SDL.Keycode.Escape))
        {
            mayAddTextInput = false;

            editorCommand = EditorCommand.Escape;

            goto returnText;
        }

        if (enterPressed)
        {
            mayAddTextInput = false;

            if (TryCooldown(SDL.Keycode.Return))
            {
                editorCommand = EditorCommand.Enter;
            }

            goto returnText;
        }

        if (tabPressed)
        {
            mayAddTextInput = false;

            if (TryCooldown(SDL.Keycode.Tab))
            {
                editorCommand = EditorCommand.Tab;
            }

            goto returnText;
        }

        if (backspacePressed)
        {
            if (newText.Length == 0) goto returnText;
            if (newCharIndex == 0) goto returnText;  

            if (TryCooldown(SDL.Keycode.Backspace))
            {
                newText = newText.Remove(newCharIndex - 1, 1);
                newCharIndex--;
            }

            goto returnText;
        }

        if (ctrlPressed)
        {
            mayAddTextInput = false;

            if (InputHandler.IsKeyPressed(SDL.Keycode.S))
            {
                editorCommand = EditorCommand.SaveFile;
                goto returnText;
            }

            if (InputHandler.IsKeyDown(SDL.Keycode.Plus))
            {
                if (TryCooldown(SDL.Keycode.Plus))
                {
                    editorCommand = EditorCommand.ZoomIn;
                }

                goto returnText;
            }

            if (InputHandler.IsKeyDown(SDL.Keycode.Minus))
            {
               if (TryCooldown(SDL.Keycode.Minus))
                {
                    editorCommand = EditorCommand.ZoomOut;
                }

                goto returnText;
            }

            if (InputHandler.IsKeyDown(SDL.Keycode.LShift))
            {
                if (InputHandler.IsKeyPressed(SDL.Keycode.P))
                {
                    editorCommand = EditorCommand.ToggleFileDialog;
                }
            }

            if (InputHandler.IsKeyDown(SDL.Keycode.X))
            {
                if (TryCooldown(SDL.Keycode.X))
                {
                    editorCommand = EditorCommand.DeleteLine;
                }
            }
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Left))
        {
            if (TryCooldown(SDL.Keycode.Left))
            {
                if (newCharIndex > 0)
                {
                    newCharIndex--;
                }
            }

            goto returnText;
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Right))
        {
            if (TryCooldown(SDL.Keycode.Right))
            {
                if (newCharIndex != newText.Length)
                {
                    newCharIndex++;
                }
            }

            goto returnText;
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Up))
        {
            if (TryCooldown(SDL.Keycode.Up))
            {
                editorCommand = EditorCommand.ArrowUp;
            }

            goto returnText;
        }

        if (InputHandler.IsKeyDown(SDL.Keycode.Down))
        {
            if (TryCooldown(SDL.Keycode.Down))
            {
                editorCommand = EditorCommand.ArrowDown;
            }

            goto returnText;
        }


        returnText:
            return new KeybindHandlerInfo(newText, newCharIndex, mayAddTextInput, editorCommand);
    }

    public static void UpdateCooldowns(double deltaTime)
    {
        foreach (KeyValuePair<SDL.Keycode, float> cooldowns in _keyCooldowns)
        {
            if (!InputHandler.IsKeyDown(cooldowns.Key))
            {
                _keyCooldowns[cooldowns.Key] = 0;
                _markedForFastCooldown[cooldowns.Key] = false;
                continue;
            }

            if (_keyCooldowns[cooldowns.Key] > 0)
            {
                _keyCooldowns[cooldowns.Key] -= (float)deltaTime;
            }
        }
    }

    private static bool TryCooldown(SDL.Keycode key)
    {
        if (_keyCooldowns.TryGetValue(key, out float cooldown))
        {
            if (cooldown <= 0)
            {
                if (!_markedForFastCooldown.TryGetValue(key, out _)) _markedForFastCooldown.Add(key, false);

                if (_markedForFastCooldown[key])
                {
                    _keyCooldowns[key] = _baseFastCooldown;
                } 
                else
                {
                    _keyCooldowns[key] = _baseCooldown;
                    _markedForFastCooldown[key] = true;
                }

                return true;
            }
        }
        else
        {
            _keyCooldowns.Add(key, _baseCooldown);
            return true;
        }

        return false;
    }
}