using Smash.Graphics;
using Smash;
using Color = System.Drawing.Color;
using System.Numerics;
using Smash.Input;
using SDL3;

public static class FileDialog
{
    public static float DIALOG_WIDHT => Math.Max(400 * App.ScaleFactor, App.Font.MeasureString(_text).X + App.PointSize * 1.5f);
    public static float DIALOG_HEIGHT => 50 * App.ScaleFactor;

    public static bool Opened = false;

    public static float DialogX;
    public static float DialogY;
    public static Vector2 DialogPosition => new Vector2(DialogX, DialogY);

    private static string _text = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "/";
    private static int _charIndex = _text.Length;

    private static Vector2 _caretPosition = new();

    private static bool _ignoreClose = false;

    private static Dictionary<string, float> _systemEntries = new();
    private static int _selectedIndex = 0;
    private static float _scroll;

    private const int MAX_ENTRY_OFFSET = 30;

    public static void Update(double deltaTime)
    {
        if (Opened)
        {
            TextInputInfo textInputInfo = TextHandler.Handle(_text, _charIndex);

            string previousText = _text;
            _text = textInputInfo.NewText;
            _charIndex = textInputInfo.NewCharIndex;

            if (previousText != _text) RefreshEntries();


            if (textInputInfo.TextInputOperation != null)
            {
                if (textInputInfo.TextInputOperation == TextInputOperation.ToggleFileDialog && !_ignoreClose)
                {
                    Opened = false;
                }

                if (textInputInfo.TextInputOperation == TextInputOperation.DeleteWord)
                {
                    int jumpCharAmount = TextOperation.JumpLeft(_text, _charIndex);
                    _text = _text.Remove(_charIndex - jumpCharAmount, jumpCharAmount);
                    _charIndex -= jumpCharAmount;
                    RefreshEntries();
                }

                if (textInputInfo.TextInputOperation == TextInputOperation.JumpLeft)
                {
                    int jumpCharAmount = TextOperation.JumpLeft(_text, _charIndex);
                    _charIndex -= jumpCharAmount;
                }

                if (textInputInfo.TextInputOperation == TextInputOperation.JumpRight)
                {
                    int jumpCharAmount = TextOperation.JumpRight(_text, _charIndex);
                    _charIndex += jumpCharAmount;
                }

                if (textInputInfo.TextInputOperation == TextInputOperation.JumpDown)
                {
                    if (_selectedIndex + 1 < _systemEntries.Keys.Count)
                    {
                        _selectedIndex++;
                    }
                }

                if (textInputInfo.TextInputOperation == TextInputOperation.JumpUp)
                {
                    if (_selectedIndex - 1 >= 0)
                    {
                        _selectedIndex--;
                    }
                }

                if (textInputInfo.TextInputOperation == TextInputOperation.DeleteCharacter)
                {
                    int deleteCharAmount = TextOperation.DeleteCharacter(_text, _charIndex);
                    if (deleteCharAmount > 1) _charIndex++;
                    _text = _text.Remove(_charIndex - deleteCharAmount, deleteCharAmount);
                    _charIndex -= deleteCharAmount;
                    RefreshEntries();
                }

                if (textInputInfo.TextInputOperation == TextInputOperation.Tab)
                {
                    if (_selectedIndex < _systemEntries.Keys.Count)
                    {
                        _text = _systemEntries.ElementAt(_selectedIndex).Key;
                        if (Directory.Exists(_systemEntries.ElementAt(_selectedIndex).Key)) _text += "/";
                    
                        _charIndex = _text.Length;
                        RefreshEntries();
                    }
                }

                if (textInputInfo.TextInputOperation == TextInputOperation.MoveLeft)
                {
                    if (_charIndex > 0)
                        _charIndex--;
                }

                if (textInputInfo.TextInputOperation == TextInputOperation.MoveRight)
                {
                    if (_charIndex + 1 <= _text.Length)
                        _charIndex++;
                }

                if (textInputInfo.TextInputOperation == TextInputOperation.NewLine)
                {
                    if (_selectedIndex < _systemEntries.Keys.Count)
                    {
                        if (File.Exists(_text))
                        {
                            Opened = false;
                            App.LoadFile(_text);
                        }
                        else if (Directory.Exists(_systemEntries.ElementAt(_selectedIndex).Key))
                        {
                            _text = _systemEntries.ElementAt(_selectedIndex).Key + "/";
                            _charIndex = _text.Length;
                            RefreshEntries();
                        }
                    }
                }

                if (textInputInfo.TextInputOperation == TextInputOperation.ForceNewLine)
                {
                    if (!File.Exists(_text) && !Directory.Exists(_text))
                    {
                        using (var fileStream = File.Create(_text)) { }
                        Opened = false;
                        App.LoadFile(_text);
                    }
                }

                if (textInputInfo.TextInputOperation == TextInputOperation.PasteClipboard)
                {
                    string clipboardText = SDL.GetClipboardText();
                    _text = _text.Insert(_charIndex, clipboardText);
                    _charIndex += clipboardText.Length;
                }
            }

            if (_ignoreClose) _ignoreClose = false;
        }

        Vector2 preferredDialogPosition = GetPreferredDialogPosition();
        if (DialogPosition != preferredDialogPosition)
        {
            DialogX = MathHelper.Lerp(DialogX, preferredDialogPosition.X, 30 * (float)deltaTime);
            DialogY = MathHelper.Lerp(DialogY, preferredDialogPosition.Y, 30 * (float)deltaTime);
        }

        Vector2 preferredCaretPosition = GetPreferredCaretPosition();
        if (_caretPosition != preferredCaretPosition)
        {
            _caretPosition = MathHelper.LerpVector(_caretPosition, preferredCaretPosition, App.CARET_SPEED * (float)deltaTime);
        }

        foreach (var entry in _systemEntries)
        {
            if (_systemEntries.ElementAt(_selectedIndex).Key == entry.Key)
            {
                _systemEntries[entry.Key] = MathHelper.Lerp(_systemEntries[entry.Key], MAX_ENTRY_OFFSET, 30 * (float)deltaTime);
            }
            else if (_systemEntries[entry.Key] > 0)
            {
                _systemEntries[entry.Key] = MathHelper.Lerp(_systemEntries[entry.Key], 0, 25 * (float)deltaTime);
            }
        }

        _scroll = MathHelper.Lerp(_scroll, Math.Max((DIALOG_HEIGHT / 1.5f * _selectedIndex) - ((App.WindowHeight - DialogY / 2) / 2), 0), 50 * (float)deltaTime);
    }

    public static void Render(Renderer renderer)
    {
        renderer.RenderFilledRectangle(new Rectangle(DialogPosition, DIALOG_WIDHT, DIALOG_HEIGHT), Color.FromArgb(40, 40, 40));

        Vector2 textPosition = DialogPosition + new Vector2(App.PointSize / 2);
        renderer.RenderText(App.Font, _text, textPosition, Color.White);

        Vector2 entryStartPosition = DialogPosition + new Vector2(0, DIALOG_HEIGHT);
        for (int i = 0; i < _systemEntries.Keys.Count; i++)
        {
            Vector2 position = entryStartPosition + new Vector2(0, DIALOG_HEIGHT / 1.5f * i);

            Vector2 entryTextPosition = position + new Vector2(App.PointSize / 4) - new Vector2(0, _scroll);
            if (entryTextPosition.Y < DialogY + DIALOG_HEIGHT || entryTextPosition.Y > App.WindowHeight) continue;

            string fileText = Path.GetFileName(_systemEntries.ElementAt(i).Key);
            if (Directory.Exists(_systemEntries.ElementAt(i).Key)) fileText += "/";
            entryTextPosition.X -= _systemEntries.ElementAt(i).Value;

            if (i == _selectedIndex)
            {
                fileText = fileText.Insert(0, "-> ");
                entryTextPosition.X -=  + App.Font.MeasureString("-> ").X;
            } 

            renderer.RenderText(App.Font, fileText, entryTextPosition, Directory.Exists(_systemEntries.ElementAt(i).Key) ? Color.RoyalBlue : Color.White);
        }

        if (Opened)
        {
            renderer.RenderFilledRectangle(new Rectangle(_caretPosition + new Vector2(-1, 3), 2 * App.ScaleFactor, App.PointSize), Color.RoyalBlue);
        }
    }

    public static void Open(string? currentDirectory)
    {
        Opened = true;
        _ignoreClose = true;
        _selectedIndex = 0;

        if (currentDirectory != null)  
        {
            _text = currentDirectory + "/";
            _charIndex = _text.Length;
        }

        RefreshEntries();
    }

    public static Vector2 GetPreferredCaretPosition()
    {
        return DialogPosition + new Vector2(App.Font.MeasureString(_text.Substring(0, _charIndex)).X, 0) + new Vector2(App.PointSize / 2);
    }

    private static Vector2 GetPreferredDialogPosition()
    {
        if (Opened)
        {
            return new Vector2(App.WindowWidth - DIALOG_WIDHT, 80);
        }
        else
        {
            return new Vector2(App.WindowWidth + 100, 80);
        }
    }

    private static void RefreshEntries()
    {
        string? directory = Path.GetDirectoryName(_text);
        _selectedIndex = 0;

        if (directory == null)
        {
            _systemEntries = [];
        }
        else
        {
            if (!Directory.Exists(directory))
            {
                _systemEntries = [];
                return;
            }

            string[] entries = Directory.GetFileSystemEntries(directory);
            _systemEntries.Clear();

            foreach (string entry in entries)
            {
                if (entry.Length >= _text.Length && entry.Substring(0, _text.Length).ToLower() == _text.ToLower())
                {
                    _systemEntries.Add(entry, 0);
                }
            }
        }
    }
}