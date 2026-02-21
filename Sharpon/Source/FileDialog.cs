using Color =  System.Drawing.Color;
using System.Numerics;
using Smash.Graphics;
using Smash;
using Smash.Input;
using SDL3;
using System.Runtime.CompilerServices;

public class FileDialog
{
    public bool Opened;

    private Vector2 _fileDialogPosition;
    private float _fileDialogWidth;
    private Vector2 _caretPosition;

    private readonly float _baseFileDialogPadding = 500;
    private readonly float _widthMargin = 5;

    private readonly Color _backgroundColor = Settings.FILE_DIALOG_BACKGROUND_COLOR;

    private string _text = "/";
    private string? _textInput;
    private int _charIndex = 1;

    private readonly Window _window;

    private Font _font = null!;
    private float _pointSize;
    private float _previousPointSize;

    private string[] _filePaths = [];
    private float _filePathsYOffset = 0;
    private int _selectedFilePath = 0;
    bool hasAccesToFilePath;

    private Vector2 _selectedFilePathBounds;
    private Vector2 _selectedFilePathPosition;
    
    public FileDialog(Window window)
    {
        _pointSize = Sharpon.PointSize - 4;

        _window = window;
        _font = Sharpon.GetOrCreateFont(_pointSize);
        _previousPointSize = _pointSize;
        _fileDialogPosition = new Vector2(1920, 100);
    }

    public void Update(double deltaTime)
    {
        if (_previousPointSize != _pointSize)
        {
            _font = Sharpon.GetOrCreateFont(_pointSize);
            _previousPointSize = _pointSize;
        }

        if (Opened)
        {
            KeybindHandlerInfo keybindHandlerInfo = KeybindHandler.HandleKeybinds(_text, _charIndex);

            _text = keybindHandlerInfo.NewText;
            _charIndex = keybindHandlerInfo.NewCharIndex;

            if (keybindHandlerInfo.EditorCommand != null)
            {
                if (keybindHandlerInfo.EditorCommand == EditorCommand.ToggleFileDialog)
                {
                    Opened = false;
                }

                if (keybindHandlerInfo.EditorCommand == EditorCommand.ZoomIn)
                {
                    _pointSize += 3;
                }

                if (keybindHandlerInfo.EditorCommand == EditorCommand.ZoomOut)
                {
                    int minPointSize = 12;
                
                    if (_pointSize - 3 <= minPointSize)
                    {
                        _pointSize = minPointSize;
                    }
                    else
                    {
                        _pointSize -= 3;
                    }
                }

                if (keybindHandlerInfo.EditorCommand == EditorCommand.ArrowDown)
                {
                    _selectedFilePath++;
                }

                if (keybindHandlerInfo.EditorCommand == EditorCommand.ArrowUp)
                {
                    _selectedFilePath--;
                    if (_selectedFilePath < 0) _selectedFilePath = 0;
                }

                if (keybindHandlerInfo.EditorCommand == EditorCommand.Tab)
                {
                    if (_filePaths.Length > 0)
                    {
                        bool isDirectory = Directory.Exists(_filePaths[_selectedFilePath]);

                        _text = _filePaths[_selectedFilePath] + (isDirectory ? "/" : "");
                        _charIndex = _text.Length;
                    }
                }

                if (keybindHandlerInfo.EditorCommand == EditorCommand.Enter)
                {
                    if (_filePaths.Length > 0)
                    {
                        bool isDirectory = Directory.Exists(_filePaths[_selectedFilePath]);

                        if (isDirectory)
                        {
                            _text = _filePaths[_selectedFilePath] + "/";
                            _charIndex = _text.Length;
                        }
                        else
                        {
                            if (File.Exists(_text))
                            {
                                Sharpon.LoadFile(_text);
                            }
                        
                            Opened = false;
                        }
                    }
                }
            }
            else if (keybindHandlerInfo.MayAddTextInput)
            {
                if (_textInput != null)
                {
                    _text = _text.Insert(_charIndex, _textInput);
                    _charIndex += _textInput.Length;
                    _selectedFilePath = 0;
                    _filePathsYOffset = 0;
                }
            }

            if (!_text.StartsWith("/"))
            {
                if (_text.Length <= 0)
                {
                    _text = "/";
                    _charIndex = 1;
                }
                else
                {
                    _text = _text.Insert(0, "/");
                }
            }

            try
            {
                _filePaths = Directory.GetFileSystemEntries(Path.GetDirectoryName(_text) ?? _text);
                _filePaths = _filePaths.OrderByDescending(f => f.Substring(0, Math.Min(_text.Length, f.Length)) == _text).ToArray();
                hasAccesToFilePath = true;
            }
            catch (UnauthorizedAccessException)
            {
                hasAccesToFilePath = false;
                _filePaths = [];
            }

            if (_filePaths.Length > 0)
            {
                if (_selectedFilePath == -1) _selectedFilePath = 0;

                _selectedFilePath = Math.Min(_filePaths.Length - 1, _selectedFilePath);
            }
            else
            {
                _selectedFilePath = -1;
            }
        }            

        Vector2 preferredFileDialogPosition = Opened ? new Vector2(_window.Width - _fileDialogWidth, 100) : new Vector2(_window.Width, 100);
        _fileDialogPosition = MathHelper.LerpVector(_fileDialogPosition, preferredFileDialogPosition, Settings.FILE_DIALOG_SPEED * (float)deltaTime); 

        _textInput = null;



        _fileDialogWidth = Math.Max(_baseFileDialogPadding * (_pointSize / Settings.FONT_POINT_SIZE), _font.MeasureString(_text).X + 10);


        Vector2 preferredCaretPosition = GetCaretPosition();
        _caretPosition = MathHelper.LerpVector(_caretPosition, preferredCaretPosition, Settings.CARET_SPEED * (float)deltaTime);

        if (_filePaths.Length > 0)
        {
            bool isDirectory = Directory.Exists(_filePaths[_selectedFilePath]);
            
            Vector2 preferredFilePathBounds = _font.MeasureString(Path.GetFileName(_filePaths[_selectedFilePath]) + (isDirectory ? "/" : "")) + new Vector2(_widthMargin) * 2;
            _selectedFilePathBounds = MathHelper.LerpVector(_selectedFilePathBounds, preferredFilePathBounds, Settings.FILE_DIALOG_SPEED * (float)deltaTime);

            float lineSpacing = _pointSize + 8;
            Vector2 preferredFilePathPosition = _fileDialogPosition + new Vector2(_widthMargin) + new Vector2(0, _pointSize * 2) + new Vector2(0, _selectedFilePath * lineSpacing) - new Vector2(_widthMargin) - new Vector2(0, _filePathsYOffset);
            _selectedFilePathPosition = MathHelper.LerpVector(_selectedFilePathPosition, preferredFilePathPosition, Settings.FILE_DIALOG_SPEED * (float)deltaTime);

            float scrollSpeed = 30;
            if (_selectedFilePathPosition.Y > _window.Height / 1.5f)
            {
                _filePathsYOffset = MathHelper.Lerp(_filePathsYOffset, _filePathsYOffset + preferredFilePathBounds.Y, scrollSpeed * (float)deltaTime);
            }
            else if (_selectedFilePathPosition.Y < _window.Height / 1.8f)
            {
                if (_filePathsYOffset > 0)
                {
                    _filePathsYOffset = MathHelper.Lerp(_filePathsYOffset, _filePathsYOffset - preferredFilePathBounds.Y, scrollSpeed * (float)deltaTime); 
                }
            }
        }
    }

    public void Render(Renderer renderer)
    {
        Rectangle backgroundRectangle = new Rectangle(_fileDialogPosition - new Vector2(_widthMargin), new Vector2(_fileDialogWidth, 30) + new Vector2(_widthMargin) * 2);
        renderer.RenderFilledRectangle(backgroundRectangle, _backgroundColor);

        renderer.RenderFilledRectangle(new Rectangle(_caretPosition, Settings.CARET_WIDTH, _font.PointSize), Color.RoyalBlue);

        Vector2 textPosition = _fileDialogPosition + new Vector2(_widthMargin);
        renderer.RenderText(_font, _text, textPosition, Color.White);

        if (!hasAccesToFilePath)
        {
            renderer.RenderText(_font, "Access to Folder denied by system", textPosition + new Vector2(0, backgroundRectangle.Height), Color.Red);
        }

        
        float lineSpacing = _pointSize + 8;
        Vector2 baseFilePathPosition = textPosition + new Vector2(0, _pointSize * 2 - _filePathsYOffset);

        SDL.Rect clipRect = new Rectangle(textPosition + new Vector2(0, _pointSize * 2) - new Vector2(_widthMargin, _widthMargin + 4), _window.Width - baseFilePathPosition.X, _window.Height).ToSDLRect();
        SDL.SetRenderClipRect(renderer.Handle, clipRect);

        for (int i = 0; i < _filePaths.Length; i++)
        {
            bool isDirectory = Directory.Exists(_filePaths[i]);
            Vector2 filePathPosition = baseFilePathPosition + new Vector2(0, i * lineSpacing);

            if (_selectedFilePath == i)
            {
                Rectangle pathBackgroundRectangle = new Rectangle(_selectedFilePathPosition, _selectedFilePathBounds);

                renderer.RenderFilledRectangle(pathBackgroundRectangle, _backgroundColor);
                renderer.RenderRectangle(pathBackgroundRectangle, Color.Orange);
            }

            renderer.RenderText(_font, Path.GetFileName(_filePaths[i]) + (isDirectory ? "/" : ""), filePathPosition, isDirectory ? Color.RoyalBlue : Color.White);
        }

        SDL.SetRenderClipRect(renderer.Handle, IntPtr.Zero);
    }

    public void SendTextInput(string? textInput)
    {
        _textInput = textInput;
    }

    private Vector2 GetCaretPosition()
    {
        return _fileDialogPosition + new Vector2(_font.MeasureString(_text.Substring(0, _charIndex)).X + _widthMargin, 2 + _widthMargin);
    }
}