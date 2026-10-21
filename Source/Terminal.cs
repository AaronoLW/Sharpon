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

    private readonly TerminalProcess _process;

    public Terminal()
    {
        _process = new();
        _process.OutputData += ReceiveData;
        _process.SendCommand("pwd");
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
    }

    public void TextEdit(IKeybindSet keybindSet)
    {
    }

    public void Render(Renderer renderer)
    {
        Rectangle backgroundRectangle = new(Position, App.WindowWidth, App.TERMINAL_HEIGHT);
        renderer.RenderFilledRectangle(backgroundRectangle, Color.Black);

        renderer.RenderLine(0, App.WindowHeight - Height, App.WindowWidth, App.WindowHeight - Height, Drip.AccentColor.ToColor());
    }

    private void ReceiveData(string data)
    {
        Console.WriteLine(data);
    }
}
