using Color = System.Drawing.Color;
using SmashFramework;
using System.Numerics;
using SDL3;

public class Terminal : IRenderable, IPermutable
{
    public bool ShouldRender { get; set; } = true;

    private readonly TextDocument _document = new("");

    private const int LINE_SIZE = 2;

    private Vector2 _startPosition => new(0, App.WindowHeight - App.TERMINAL_HEIGHT);
    private Vector2 _textPosition => _startPosition + new Vector2(App.DEFAULT_PADDING / 2);

    private float _lineSpacing => App.POINT_SIZE + App.LINE_SPACING_POINT_SIZE_INCREASE;

    public void Update(double deltaTime)
    {
        if (Program.TextInput != null)
        {
            _document.Insert(Program.TextInput);

            char? insert = null;
            if (Program.TextInput == "{") insert = '}';
            if (Program.TextInput == "(") insert = ')';
            if (Program.TextInput == "\"") insert = '"';
            if (Program.TextInput == "[") insert = ']';

            if (insert != null)
            {
                _document.Insert((char)insert);
                _document.CharIndex--;

            }
        }

        //_keybindHandler.HandleKeybinds(_document);

        if (Program.IsKeyDown(SDL.Keycode.Return))
        {
            HandleReturn();
        }
    }

    public void Render(Renderer renderer)
    {
        renderer.RenderFilledRectangle(new(_startPosition, App.WindowWidth, App.TERMINAL_HEIGHT), Color.Black);
        renderer.RenderFilledRectangle(new(_startPosition.X, _startPosition.Y - LINE_SIZE / 2, App.WindowWidth, LINE_SIZE), Color.RoyalBlue);

        renderer.RenderText(App.Font, App.POINT_SIZE, _document.Text, _textPosition, Color.White);

        Rectangle caret = new(_document.GetCaretPosition(_textPosition, new(App.POINT_SIZE, _lineSpacing)), 2, App.POINT_SIZE);
        renderer.RenderFilledRectangle(caret, Color.RoyalBlue);
    }

    private void HandleReturn()
    {
        _document.Clear();
    }
}
