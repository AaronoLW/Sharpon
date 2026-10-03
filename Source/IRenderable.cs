using SmashFramework;

public interface IRenderable
{
    public bool ShouldRender { get; set; }
    public void Render(Renderer renderer);
}
