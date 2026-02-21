using System.Runtime.InteropServices;
using SDL3;
using Smash;
using Smash.Input;

internal static class Program
{
    public static event EventHandler<WindowResizeEventArgs>? WindowResize;

    private static DeltaTimeCounter _deltaTimeCounter = new();
    private static InputHandler _inputHandler = new();

    private static void Main(string[] args)
    {
        SDL.Init(SDL.InitFlags.Video);

        Sharpon application = new Sharpon();
        application.Start();

        if (args.Length > 1 || args.Length < 1)
        {
            Sharpon.LoadFile(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "sharpon.txt"), true);
            Console.WriteLine("Loading sharpon.txt");
        }
        else
        {
            string path = Path.GetFullPath(args[0]);
            
            Sharpon.LoadFile(path);
            Console.WriteLine($"Loading {Path.GetFileName(path)}");
        }

        bool running = true;
        while (running)
        {
            _deltaTimeCounter.Update();
            _inputHandler.Update();
            KeybindHandler.UpdateCooldowns(_deltaTimeCounter.Seconds);

            while (SDL.PollEvent(out SDL.Event e))
            {
                if (e.Type == (uint)SDL.EventType.Quit)
                {
                    running = false;
                }

                if (e.Type == (uint)SDL.EventType.WindowResized)
                {
                    WindowResize?.Invoke(null, new WindowResizeEventArgs(e.Window.WindowID));
                }

                if (e.Type == (uint)SDL.EventType.TextInput)
                {
                    application.SendTextInput(Marshal.PtrToStringUTF8(e.Text.Text)!);
                }

                _inputHandler.Event(e);
            }

            application.Update(_deltaTimeCounter.Seconds);
            application.Render();
        }

        application.End();
        SDL.Quit();
    }
}