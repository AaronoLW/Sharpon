using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using FontStashSharp;
using System.IO;
using System;
using MonoGame.Extended;
using System.Xml;
using System.Runtime.CompilerServices;

namespace Sharpon;

public class Game1 : Game
{
    public static SpriteFontBase Font;
    private UISystem _uiSystem;

    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Color _backgroundColor = new Color(30, 30, 33);
    private Color _fileDialogColor = new Color(40, 40, 43);
    private Color _filePathDialogColor = new Color(50, 50, 53);
    private FontSystem _fontSystem;
    private string _fontFilePath;
    private int _previousFontSize;

    private static Editor _editor;
    private static FileDialog _fileDialog;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _uiSystem = new UISystem(Window);
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        
        _fontSystem = new FontSystem();
        _fontFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Fonts", "JetBrainsMono-Bold.ttf");
        _fontSystem.AddFont(File.ReadAllBytes(_fontFilePath));
    
        _editor = new Editor(_uiSystem, new Vector2(30, 30), "yo\nyo2", _backgroundColor);
        _editor.ReceiveTextInput = false;
        _editor.OpenFile("/media/TatzisUnterlagen/sharpon.txt");

        Vector2 fileDialogPosition = Window.ClientBounds.Location.ToVector2() + Window.ClientBounds.Size.ToVector2() / 2 - new Vector2(100, 300);
        _fileDialog = new FileDialog(_uiSystem, fileDialogPosition, "/", _fileDialogColor, _filePathDialogColor);
        _fileDialog.SetCharIndex(1);

        _fileDialog.OnClose += OnCloseFileDialog;
        _fileDialog.OnEnter += OnEnterFileDialog;
    }

    protected override void Update(GameTime gameTime)
    {
        _uiSystem.Update(gameTime);
        _uiSystem.PollMiscellanousKeys();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(_backgroundColor);

        _spriteBatch.Begin();

        if (_previousFontSize != Settings.FontSize)
        {
            Font = _fontSystem.GetFont(Settings.FontSize);
        }
        _previousFontSize = Settings.FontSize;

        string[] lines = _editor.Text.Split("\n");
        for (int i = 0; i < lines.Length; i++) _spriteBatch.DrawString(Font, lines[i], _editor.Position + (i * Settings.SpacingVector), Color.White);
        
        Vector2 caretPosition = _editor.CalculateCaretPosition(_editor.CharIndex);
        _spriteBatch.DrawString(Font, "|", caretPosition, Color.White);

        if (_fileDialog != null)
        {
            _fileDialog.Draw(_spriteBatch);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    public static void DisableTextReceiving()
    {
        if (_editor != null) _editor.ReceiveTextInput = false;
        //if (_fileDialog != null) _fileDialog.ReceiveTextInput = false;
    }

    private void OnCloseFileDialog()
    {
        DisableTextReceiving();
        if (_editor != null) _editor.ReceiveTextInput = true;
    }

    private void OnEnterFileDialog()
    {
        if (_editor != null) _editor.OpenFile(_fileDialog.Text);
    }
}