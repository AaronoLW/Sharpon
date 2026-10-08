using SDL3;
using SmashFramework;

public static class KeybindHelper
{
    public static SDL.Keycode[] GetPressedModifiers()
    {
        List<SDL.Keycode> modifiers = [];

        if (Input.IsKeyDown(SDL.Keycode.LShift))
            modifiers.Add(SDL.Keycode.LShift);

        if (Input.IsKeyDown(SDL.Keycode.LCtrl))
            modifiers.Add(SDL.Keycode.LCtrl);

        return [.. modifiers];
    }
}
