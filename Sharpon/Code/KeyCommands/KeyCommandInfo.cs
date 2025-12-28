using Microsoft.Xna.Framework.Input;

public class KeyCommandInfo
{
    public bool Repeats;
    public float RepeatCooldown;
    public bool KeyHeld;
    public bool DidSlowCooldown;
    public Keys AssignedKey;

    public KeyCommandInfo(bool repeats, Keys key)
    {
        Repeats = repeats;
        AssignedKey = key;
    }
}