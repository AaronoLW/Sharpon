using SDL3;
using Smash.Input;

public static class KeybindHandler
{
    private static Dictionary<SDL.Keycode, float> _keyCooldowns = new();
    private static Dictionary<SDL.Keycode, bool> _markedForFastCooldown = new();

    private static readonly float _baseCooldown = 0.08f;
    private static readonly float _baseFastCooldown = 0.03f;

    public static KeybindHandlerInfo HandleKeybinds(string initialText, int charIndex)
    {
        bool ctrlPressed = InputHandler.IsKeyDown(SDL.Keycode.LCtrl);
        bool enterPressed = InputHandler.IsKeyDown(SDL.Keycode.Return);
        bool backspacePressed = InputHandler.IsKeyDown(SDL.Keycode.Backspace);

        string newText = initialText;
        int newCharIndex = charIndex;

        EditorCommand? editorCommand = null;
        bool mayAddTextInput = true;

        if (enterPressed)
        {
            if (TryCooldown(SDL.Keycode.Return))
            {
                newText += "\n";
                newCharIndex++;
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
            if (InputHandler.IsKeyPressed(SDL.Keycode.S))
            {
                editorCommand = EditorCommand.SaveFile;
                goto returnText;
            }

            mayAddTextInput = false;
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