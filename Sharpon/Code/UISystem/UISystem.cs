using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public class UISystem
{
    private static List<UIElement> _uiElements = new List<UIElement>();
    private GameWindow _gameWindow;

    private static Keys[] _miscellanousKeys =
    {
        Keys.Left,
        Keys.Right,
        Keys.Up,
        Keys.Down,
        Keys.P,
        Keys.Tab,
        Keys.Enter,
        Keys.Escape
    };

    public UISystem(GameWindow gameWindow)
    {
        _gameWindow = gameWindow;
    }

    public void Register(UIElement uiElement)
    {
        _gameWindow.TextInput += uiElement.OnTextInput;
        _uiElements.Add(uiElement);
        Console.WriteLine("Registered " + uiElement.GetType());
    }

    public void Update(GameTime gameTime)
    {
        foreach (UIElement element in _uiElements)
        {
            element.UpdateKeybindHandler(gameTime);
        }
    }

    public void PollMiscellanousKeys()
    {
        KeyboardState keyboardState = Keyboard.GetState();

        foreach (Keys key in _miscellanousKeys)
        {
            if (keyboardState.IsKeyDown(key))
            {
                foreach (UIElement element in _uiElements)
                {
                    element.HandleMiscellaneousKey(key);
                }
            }
        }
    }
}