using System.Numerics;
using SmashFramework;

public class GUIRoot : GUIElement
{
    private readonly List<GUIElement> _children;

    public GUIRoot(Vector2 position, params List<GUIElement> children)
    {
        Position = position;

        _children = children;
        RecalculatePositions();
    }

    public void RecalculatePositions()
    {
        float yOffset = 0;

        for (int i = 0; i < _children.Count; i++)
        {
            GUIElement element = _children[i];
            element.Position = Position + new Vector2(0, yOffset);
            yOffset += element.Height;
        }
    }

    public override void Render(Renderer renderer)
    {
        foreach (var child in _children)
            child.Render(renderer);
    }
}
