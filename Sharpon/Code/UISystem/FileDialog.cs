using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Sharpon;
using FontStashSharp;
using System;
using System.Linq;
using System.Text.Encodings.Web;

public class FileDialog : UIElement
{
    private float _textPadding { get; } = 5;
    private float _entrySpacing { get; } = 24;
    private string[] _filePaths;
    private string previousText;
    private Color _pathBackgroundColor;
    private int _selectedIndex = 0;
    public bool IsOpened = false;
    
    public event Action OnClose;
    public event Action OnEnter;

    public FileDialog(UISystem uiSystem, Vector2 position, string text, Color backgroundColor, Color pathBackgroundColor) : base(uiSystem, position, text, backgroundColor)
    {
        _pathBackgroundColor = pathBackgroundColor;
        _keybindHandler = new KeybindHandler(new OpenFileDialogCommand(),
                                             new LeftArrowCommand(),
                                             new RightArrowCommand(),
                                             new DownArrowCommand((UIElement uiElement) =>
                                             {
                                                 FileDialog dialog = uiElement as FileDialog;
                                                 dialog._selectedIndex++;
                                                 dialog._selectedIndex = Math.Min(dialog._selectedIndex, dialog.GetFilePaths() == null ? 0 : dialog.GetFilePaths().Length - 1);
                                             }),
                                             new UpArrowCommand((UIElement uiElement) =>
                                             {
                                                 FileDialog dialog = uiElement as FileDialog;
                                                 dialog._selectedIndex--;
                                                 dialog._selectedIndex = Math.Max(dialog._selectedIndex, 0);
                                             }),
                                             new BackspaceCommand(),
                                             new BackspaceExCommand(),
                                             new TabCommand((UIElement uiElement) =>
                                             {
                                                FileDialog dialog = uiElement as FileDialog;
                                                string[] filePaths = dialog.GetFilePaths();
                                                if (filePaths == null) return;

                                                int length = Path.GetFileName(dialog.Text).Length;
                                                dialog.RemoveTextAt(dialog.CharIndex, length);
                                                dialog.SetCharIndex(dialog.CharIndex - length);

                                                string name = Path.GetFileName(filePaths[dialog._selectedIndex]) + (Directory.Exists(filePaths[dialog._selectedIndex]) ? "/" : "");
                                                dialog.SetText(dialog.Text + name);
                                                dialog.SetCharIndex(dialog.CharIndex + name.Length);
                                             }),
                                             new EnterCommand((UIElement uIElement) =>
                                             {
                                                Enter(uIElement.Text);
                                             }),
                                             new EscapeCommand((UIElement uiElement) =>
                                             {
                                                 Close();
                                             }));
    }

    public string[] GetFilePaths()
    {
        if (previousText == Text)
        {
            return _filePaths;
        }
        previousText = Text;

        string directoryName = Path.GetDirectoryName(Text);

        if (Directory.Exists(directoryName))
        {
            _filePaths = Directory.GetFileSystemEntries(directoryName)
                                .OrderBy(p => !Path.GetFileName(p).Contains(Path.GetFileName(Text), StringComparison.OrdinalIgnoreCase)).ToArray();
            _selectedIndex = 0;
            return _filePaths;
        }

        _filePaths = null;
        return null;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (!IsOpened) return;
        string[] _filePaths = GetFilePaths() ?? [];

        float width = Math.Max(_filePaths.Length > 0 ? Game1.Font.MeasureString(_filePaths.MaxBy(c => c.Length)).X : 20f, Game1.Font.MeasureString(Text).X + 10);
        float height = _filePaths.Length * _entrySpacing;

        //Text
        spriteBatch.FillRectangle(new RectangleF(X,
                                                 Y,
                                                 width,
                                                 _entrySpacing + _textPadding * 2),
                                                 _pathBackgroundColor);
        spriteBatch.DrawString(Game1.Font, Text, Position + new Vector2(_textPadding), Color.White);;
        spriteBatch.DrawString(Game1.Font, "|", CalculateCaretPosition(CharIndex) + new Vector2(Game1.Font.MeasureString("|").X / 2, Game1.Font.MeasureString("|").Y / 4), Color.White);

        if (_filePaths.Length == 0) return;

        //_Filepaths
        height += _textPadding + 3;
        spriteBatch.FillRectangle(new RectangleF(X,
                                                 Y + _entrySpacing + _textPadding * 2,
                                                 width,
                                                 height),
                                                 BackgroundColor);

        
        for (int i = 0; i < _filePaths.Length; i++)
        {
            Vector2 position = Position + new Vector2(_textPadding, _textPadding + 10) + new Vector2(0, (i + 1) * _entrySpacing);
            Color color = Directory.Exists(_filePaths[i]) ? Color.RoyalBlue : Color.White;

            spriteBatch.DrawString(Game1.Font, Path.GetFileName(_filePaths[i]) + (Directory.Exists(_filePaths[i]) ? "/" : ""), position, color);
        
            if (i == _selectedIndex)
            {
                spriteBatch.FillRectangle(new RectangleF(position.X - 0.5f,
                                          position.Y - 0.5f,
                                          Game1.Font.MeasureString(Path.GetFileName(_filePaths[i]) + (Directory.Exists(_filePaths[i]) ? "/" : "")).X + 1,
                                          Game1.Font.MeasureString(Path.GetFileName(_filePaths[i]) + (Directory.Exists(_filePaths[i]) ? "/" : "")).Y + 1),
                                          Color.LightBlue * 0.5f);
            }
        }
    }

    public void Open()
    {
        Game1.DisableTextReceiving();
        IsOpened = true;
        ReceiveTextInput = true;
    }

    public void Close()
    {
        IsOpened = false;
        OnClose?.Invoke();
    }

    protected override void HandleTextInput(TextInputEventArgs e)
    {
        if (!IsOpened) return;
        base.HandleTextInput(e);
    }

    private void Enter(string path)
    {
        Close();
        OnEnter.Invoke();

        SetText(Path.GetDirectoryName(Text) + "/");
        SetCharIndex(Text.Length);
    }
}