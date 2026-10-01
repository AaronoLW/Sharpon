using SmashFramework;

public class Scroller(float? min = null, float? max = null)
{
    public const int SCROLL_AMPLIFIER = 40;
    public const int SCROLL_SPEED = 30;

    private readonly float? _minimum = min;
    private readonly float? _maximum = max;

    public float Scroll { get; private set; } = 0;
    private float _preferredScroll;

    public void Update()
    {
        if (Input.ScrollWheelDelta != 0)
        {
            _preferredScroll -= Input.ScrollWheelDelta * SCROLL_AMPLIFIER;
        }

        if (_minimum != null)
            if (_preferredScroll < _minimum)
                _preferredScroll = (float)_minimum;

        if (_maximum != null)
            if (_preferredScroll > _maximum)
                _preferredScroll = (float)_maximum;
    }

    public void Animate(double deltaTime)
    {
        Scroll = MathHelper.Larp(Scroll, _preferredScroll, SCROLL_SPEED * (float)deltaTime);
    }

    public void Reset()
    {
        Scroll = 0;
        _preferredScroll = 0;
    }
}
