using Color = System.Drawing.Color;
using SmashFramework;
using System.Numerics;
using KatziDrip;

public class Terminal : IPermutable, IRenderable, IRequireTextEditing
{
    public bool Opened;

    private bool _shouldRender = false;
    public bool ShouldRender { get => _shouldRender || Height >= 1; set => _shouldRender = value; }

    public const int MAX_HEIGHT = 350;

    public float Height { get; private set; } = 0;

    public Vector2 Position => new(0, App.WindowHeight - Height);
    public Vector2 TextPosition => Position + new Vector2(App.DEFAULT_PADDING);
    public Vector2 InputFieldPosition => new(0, Position.Y + (App.TERMINAL_HEIGHT - INPUT_FIELD_HEIGHT));

    public const int INPUT_FIELD_HEIGHT = 60;
    private readonly InputField _inputField;

    private readonly TerminalProcess _process;

    public Terminal()
    {
        _process = new();
        _process.OutputData += ReceiveData;

        _inputField = new()
        {
            Prefix = "$",
            Width = App.WindowWidth,
            Height = INPUT_FIELD_HEIGHT,
        };
    }

    public void Update(double deltaTime)
    {
        if (Opened)
        {
            Height = MathHelper.Larp(Height, App.TERMINAL_HEIGHT, App.ANIMATION_SPEED * (float)deltaTime);
        }
        else
        {
            Height = MathHelper.Larp(Height, 0, App.ANIMATION_SPEED * (float)deltaTime);
        }

        _inputField.Position = InputFieldPosition;
    }

    public void Render(Renderer renderer)
    {
        Rectangle backgroundRectangle = new(Position, App.WindowWidth, App.TERMINAL_HEIGHT);
        renderer.RenderFilledRectangle(backgroundRectangle, Color.Black);

        renderer.RenderLine(0, Position.Y, App.WindowWidth, Position.Y, Drip.AccentColor.ToColor());

        renderer.RenderLine(InputFieldPosition, InputFieldPosition with { X = App.WindowWidth }, Drip.AccentColor.ToColor());
        _inputField.Render(renderer);
    }

    public void TextEdit(IKeybindSet keybindSet)
    {
        _inputField.Text += Program.TextInput;
    }

    private void ReceiveData(string data)
    {
        Console.WriteLine(data);
    }
}
