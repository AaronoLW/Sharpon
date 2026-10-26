public static class PointSizeDeleter
{
    private static Queue<float> _scheduledPointSizeDeletions = new();

    public static void Enqueue(float pointSize)
    {
        if (!_scheduledPointSizeDeletions.Contains(pointSize))
            _scheduledPointSizeDeletions.Enqueue(pointSize);
    }

    public static void Flush()
    {
        while (_scheduledPointSizeDeletions.Count > 0)
        {
            float pointSize = _scheduledPointSizeDeletions.Dequeue();
            FontManager.FontRegular.DeleteSizeIfExists(pointSize);
        }
    }
}
