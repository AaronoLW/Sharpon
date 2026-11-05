using System.Numerics;
using SmashFramework;

public abstract class GUIElement
{
    public float X { get; set; }
    public float Y { get; set; }
    public Vector2 Position
    {
        get => new(X, Y);
        set { X = value.X; Y = value.Y; }
    }

    public virtual float Width { get; set; }
    public virtual float Height { get; set; }
    public Vector2 Size
    {
        get => new(Width, Height);
        set { Width = value.X; Height = value.Y; }
    }

    public abstract void Render(Renderer renderer);
}
