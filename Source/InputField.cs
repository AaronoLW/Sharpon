using Color = System.Drawing.Color;
using System.Numerics;
using SmashFramework;
using KatziDrip;
using System.Text;

public class InputField : TextContainer
{
    public override string Text => SelectedLine;
    public string? Prefix = "";

    public Vector2 PrefixOffset => GetPrefixOffset();
    public Vector2 TextOffset => GetTextOffset();

    public float X;
    public float Y;
    public Vector2 Position
    {
        get => new(X, Y);
        set { X = value.X; Y = value.Y; }
    }

    public float Width;
    public float Height;
    public Vector2 Size
    {
        get => new(Width, Height);
        set { Width = value.X; Height = value.Y; }
    }

    public float Padding = 0;

    public event Action? OnEnter;

    public void Render(Renderer renderer)
    {
        if (Prefix != null && Prefix != string.Empty)
        {
            renderer.RenderString(FontManager.FontRegular, App.POINT_SIZE, Prefix, Vector2.Round(Position + PrefixOffset), Color.White);
        }

        renderer.RenderString(FontManager.FontRegular, App.POINT_SIZE, Text, Vector2.Round(Position + TextOffset), Color.White);
    }

    public void RenderCaret(Renderer renderer)
    {
        Vector2 caretPosition = GetCaretPosition(Position) + TextOffset;

        if (CaretStyle == CaretStyle.Beam)
        {
            Rectangle caret = new(caretPosition, 2, FontManager.FontRegular.MeasureString("|", PointSize).Y);
            renderer.RenderFilledRectangle(caret, Drip.AccentColor.ToColor());
        }
        else if (CaretStyle == CaretStyle.Block)
        {
            Rectangle caret = new(caretPosition, FontManager.FontRegular.MeasureString("|", PointSize).X, FontManager.FontRegular.MeasureString("|", PointSize).Y);
            renderer.RenderFilledRectangle(caret, Drip.AccentColor.ToColor());
        }
        else if (CaretStyle == CaretStyle.Underline)
        {
            Rectangle caret = new(caretPosition + new Vector2(0, PointSize), FontManager.FontRegular.MeasureString("|", PointSize).X, 4);
            renderer.RenderFilledRectangle(caret, Drip.AccentColor.ToColor());
        }
    }

    private Vector2 GetTextOffset()
    {
        Vector2 textOffset = new(Padding);

        textOffset += new Vector2(PrefixOffset.X, 0);

        return textOffset;
    }

    private Vector2 GetPrefixOffset()
    {
        if (Prefix == null)
            return Vector2.Zero;

        Vector2 prefixOffset = new(Padding);

        return prefixOffset;
    }

    public override void Insert(string[] lines)
    {
        StringBuilder sb = new();
        foreach (string line in lines) sb.Append(line);
        Insert(sb.ToString());
    }

    public override void InsertNewLine(bool fancyStuff = true) { OnEnter?.Invoke(); }
    public override void InsertNewLineOnPreviousLine() { }
}
