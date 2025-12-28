using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

public abstract class UIElement
{
    public float X { get; private set; }
    public float Y { get; private set; }
    public Vector2 Position
    {
        get { return new Vector2(X, Y); }
        private set { X = value.X; Y = value.Y; }
    }

    public string Text { get; private set; }
    public int CharIndex { get; private set; }
    public Color BackgroundColor { get; private set; }
    public bool ReceiveTextInput = false;

    protected KeybindHandler _keybindHandler;

    public UIElement(UISystem uiSystem, Vector2 position, string text, Color backgroundColor)
    {
        Position = position;
        Text = text;
        BackgroundColor = backgroundColor;
        _keybindHandler = new KeybindHandler();

        uiSystem.Register(this);
    }

    public void OnTextInput(object sender, TextInputEventArgs e)
    {
        if (!ReceiveTextInput) return;
        HandleTextInput(e);
    }
    
    protected virtual void HandleTextInput(TextInputEventArgs e)
    {
        KeyboardState keyboardState = Keyboard.GetState();

        if (char.IsControl(e.Character))
        {
            if (keyboardState.IsKeyDown(Keys.LeftControl))
            {
                if (e.Character == '\b') _keybindHandler.Execute<BackspaceExCommand>(this);
                if (e.Character == '\n' || e.Character == '\r') _keybindHandler.Execute<NewLineExCommand>(this); 
                return;
            }

            if (e.Character == '\n' || e.Character == '\r') _keybindHandler.Execute<NewLineCommand>(this); 
            if (e.Character == '\b') _keybindHandler.Execute<BackspaceCommand>(this);
            return;
        }

        InsertTextAt(CharIndex, e.Character.ToString());
        SetCharIndex(CharIndex + 1);
    }

    public void HandleMiscellaneousKey(Keys key)
    {
        if (key == Keys.Left) _keybindHandler.Execute<LeftArrowCommand>(this);
        if (key == Keys.Right) _keybindHandler.Execute<RightArrowCommand>(this);
        if (key == Keys.Up) _keybindHandler.Execute<UpArrowCommand>(this);
        if (key == Keys.Down) _keybindHandler.Execute<DownArrowCommand>(this);
    }
    
    public void InsertTextAt(int charIndex, string text)
    {
        if (charIndex > Text.Length) throw new IndexOutOfRangeException("CharIndex is out of range of the existing text of " + GetType());
        Text = Text.Insert(charIndex, text);
    }

    public void RemoveTextAt(int charIndex, int length)
    {
        if (charIndex > Text.Length) throw new IndexOutOfRangeException("CharIndex is out of range of the existing text of " + GetType());
        if (Text.Length - length < 0) length = Text.Length; 
        if (CharIndex == 0) return;

        Text = Text.Remove(charIndex - length, length);
    }

    public void SetCharIndex(int charIndex)
    {
        CharIndex = charIndex;
    }

    public void UpdateKeybindHandler(GameTime gameTime)
    {
        _keybindHandler.Update(gameTime);
    }
}