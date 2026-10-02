using SDL3;

public class PlaytimeCounter : IDisposable
{
    private const int AUTO_SAVE_INTERVAL = 60;

    private static readonly string SAVE_FILE_PATH = Path.Combine(App.DATA_DIRECTORY_PATH, "Playtime");

    private float _totalElapsedTime;
    private float _counter;

    public PlaytimeCounter()
    {
        if (!File.Exists(SAVE_FILE_PATH))
        {
            using var _ = File.Create(SAVE_FILE_PATH);
        }
        else
        {
            _totalElapsedTime = float.Parse(File.ReadAllText(SAVE_FILE_PATH));
        }
    }

    public void Update(double deltaTime)
    {
        _counter += (float)deltaTime;
        _totalElapsedTime += (float)deltaTime;

        if (_counter >= AUTO_SAVE_INTERVAL)
        {
            SaveToFile();
            _counter = 0;
        }
    }

    private void SaveToFile()
    {
        File.WriteAllText(SAVE_FILE_PATH, _totalElapsedTime.ToString());
    }

    public void Dispose()
    {
        SaveToFile();
    }
}
