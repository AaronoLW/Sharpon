public readonly struct EditorStyle(int pointSize, float lineSpacing)
{
    public int PointSize { get; } = pointSize;
    public float LineSpacing { get; } = lineSpacing;
}
