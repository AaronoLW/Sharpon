using System.Runtime.InteropServices;
using SDL3;
using SmashFramework;

internal static class Program
{
    private static readonly HashSet<SDL.Keycode> _downKeys = [];
    public static string? TextInput { get; private set; }

    private static void Main(string[] args)
    {
        SmashEngine.Init();

        if (args.Length > 1)
        {
            Console.WriteLine("Too many arguments");
            return;
        }

        Application application = new App(args.Length > 0 ? args[0] : null);
        application.Start();

        bool running = true;
        while (running)
        {
            SmashEngine.Update(false);

            _downKeys.Clear();
            TextInput = null;

            Input.Update();
            while (SDL.PollEvent(out SDL.Event e))
            {
                if (e.Type == (uint)SDL.EventType.KeyDown)
                {
                    _downKeys.Add(e.Key.Key);
                }

                if (e.Type == (uint)SDL.EventType.Quit)
                {
                    running = false;
                }

                if (e.Type == (uint)SDL.EventType.TextInput)
                {
                    TextInput = Marshal.PtrToStringUTF8(e.Text.Text);
                }

                Input.Event(e);
            }


            application.Update(SmashEngine.DeltaTime);
            application.Render();
        }

        application.End();
        SmashEngine.Stop();
    }

    // This in contrast to Input.IsKeyDown respects the OS' key repetition
    public static bool IsKeyDown(SDL.Keycode key) => _downKeys.Contains(key);
}
