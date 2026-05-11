using Smash;
using Smash.Input;

internal static class Program
{
    private static void Main(string[] args)
    {
        SmashEngine.Init();

        string? initialFile = null;
        if (args.Length == 1)
        {
            initialFile = Path.GetFullPath(args[0]);
        }

        Application application = new App(initialFile);
        application.Start();

        InputHandler.StartPollingTextInput();
        while (!application.ApplicationShouldClose())
        {
            SmashEngine.Update();

            application.Update(SmashEngine.DeltaTime);
            application.Render();
        }

        application.End();
        SmashEngine.Stop();
    }
}