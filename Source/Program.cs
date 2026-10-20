using System.Runtime.InteropServices;
using SDL3;
using SmashFramework;

internal static class Program
{
    private static readonly HashSet<SDL.Keycode> _downKeys = [];
    public static string? TextInput { get; private set; }

    public const int FramesPerSecond = 400;
    private readonly static double _targetFrameTime = 1000.0 / FramesPerSecond;

    private static void Main(string[] args)
    {
        Smash.Init();

        if (args.Length > 1)
        {
            Console.WriteLine("Too many arguments");
            return;
        }

        App application = new(args.Length > 0 ? args[0] : null);

        ulong lastTime = SDL.GetPerformanceCounter();

        bool running = true;
        while (running)
        {
            ulong currentTime = SDL.GetPerformanceCounter();
            double deltaTime = (double)(currentTime - lastTime) / SDL.GetPerformanceFrequency();
            lastTime = currentTime;

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

                if (e.Type == (uint)SDL.EventType.WindowResized)
                {
                    application.SetWindowSize(e.Window.Data1, e.Window.Data2);
                }

                Input.Event(e);
            }

            application.Update(deltaTime);
            application.Render();


            int sleepTime = (int)(_targetFrameTime - deltaTime);
            if (sleepTime > 0)
            {
                Thread.Sleep(sleepTime);
            }
        }

        application.End();
        Smash.Quit();
    }

    // This in contrast to Input.IsKeyDown respects the OS' key repetition
    public static bool IsKeyDown(SDL.Keycode key) => _downKeys.Contains(key);
}
