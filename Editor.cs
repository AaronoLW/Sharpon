using System.Text;
using SDL3;
using SmashFramework;

using Color = System.Drawing.Color;

public class Editor(string initialText = "")
{
    public string Text { get; private set; } = initialText;
    public int CharIndex = 0;

    public void Update(double deltaTime)
    {
        StringBuilder stringBuilder = new(Text);
        if (Input.TextInput != null)
        {
            stringBuilder.Append(Input.TextInput);
        }

        if (Input.IsKeyPressed(SDL.Keycode.Backspace))
        {
            if (stringBuilder.Length > 0)
            {
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
            }
        }

        Text = stringBuilder.ToString();
    }

    public void Render(Renderer renderer)
    {
        renderer.RenderText(App.Font, App.POINT_SIZE, Text, new(20), Color.White);
    }
}
