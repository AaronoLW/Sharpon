using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public class KeybindHandler
{
    private Dictionary<IKeyCommand, KeyCommandInfo> _commands = new();

    public KeybindHandler(params IKeyCommand[] commands)
    {
        foreach (IKeyCommand command in commands)
        {
            _commands[command] = new KeyCommandInfo(command.Repeats, command.AssignedKey);
        }
    }

    public void Execute<T>(UIElement uiElement) where T : IKeyCommand
    {
        KeyboardState keyboardState = Keyboard.GetState();

        IKeyCommand command = _commands.Keys.First(c => c.GetType().Equals(typeof(T)));
        if (command == null) return;

        _commands.TryGetValue(command, out KeyCommandInfo info);
        if (info.Repeats && info.RepeatCooldown > 0) return;
        if (info.Repeats) info.RepeatCooldown = info.KeyHeld && info.DidSlowCooldown ? Settings.ArrowKeyBaseFastCooldown : Settings.ArrowKeyBaseCooldown;
        if (info.Repeats) info.DidSlowCooldown = true;

        if (!command.RequiresControl && !keyboardState.IsKeyDown(Keys.LeftControl) || command.RequiresControl && keyboardState.IsKeyDown(Keys.LeftControl)) command?.Execute(uiElement);
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        KeyboardState keyboardState = Keyboard.GetState();
        foreach (KeyValuePair<IKeyCommand, KeyCommandInfo> keyValuePair in _commands)
        {
            KeyCommandInfo info = keyValuePair.Value;
            if (info.Repeats)
            {
                if (info.RepeatCooldown > 0) info.RepeatCooldown -= deltaTime;
                if (keyboardState.IsKeyDown(info.AssignedKey))
                {
                    info.KeyHeld = true;
                }
                else
                {
                    info.KeyHeld = false;
                    info.DidSlowCooldown = false;
                    info.RepeatCooldown = 0;
                }
            }
        }
    }
}