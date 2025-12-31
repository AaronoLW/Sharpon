using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public class KeybindHandler
{
    private Dictionary<KeyCommand, KeyCommandInfo> _commands = new();

    public KeybindHandler(params KeyCommand[] commands)
    {
        foreach (KeyCommand command in commands)
        {
            _commands[command] = new KeyCommandInfo(command.Repeats, command.AssignedKey);
        }
    }

    public void Execute<T>(UIElement uiElement) where T : KeyCommand
    {
        KeyboardState keyboardState = Keyboard.GetState();

        //IKeyCommand command = _commands.Keys.FirstOrDefault(c => c.GetType().Equals(typeof(T)));
        KeyCommand command = _commands.Keys.OfType<T>().FirstOrDefault();
        if (command == null) return;

        _commands.TryGetValue(command, out KeyCommandInfo info);
        if (info.Repeats && info.RepeatCooldown > 0) return;
        if (info.Repeats) info.RepeatCooldown = info.KeyHeld && info.DidSlowCooldown ? Settings.ArrowKeyBaseFastCooldown : Settings.ArrowKeyBaseCooldown;
        if (info.Repeats) info.DidSlowCooldown = true;

        if (!info.Repeats && info.KeyHeld) return;
        if (!info.Repeats) info.KeyHeld = true;

        if (command.RequiresControl && keyboardState.IsKeyDown(Keys.LeftControl)) 
        { 
            if (command.RequiresShift && keyboardState.IsKeyDown(Keys.LeftShift))
            {
                command?.Execute(uiElement);
                return;    
            }
            else if (!keyboardState.IsKeyDown(Keys.LeftShift)  && command.RequiresShift) return;

            command?.Execute(uiElement);
            return;
        }
        else if (!keyboardState.IsKeyDown(Keys.LeftControl) && command.RequiresControl) return;

        command?.Execute(uiElement);
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        KeyboardState keyboardState = Keyboard.GetState();
        foreach (KeyValuePair<KeyCommand, KeyCommandInfo> keyValuePair in _commands)
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
            else
            {
                if (!keyboardState.IsKeyDown(info.AssignedKey)) info.KeyHeld = false;
            }
        }
    }
}