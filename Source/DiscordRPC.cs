using DiscordRPC;

public class RichPresence : IDisposable
{
    private const string APPLICATION_ID = "1441930534052827267";

    private readonly DiscordRpcClient _client = new(APPLICATION_ID);

    public RichPresence()
    {
        _client.SetPresence(new()
        {
            Details = "Sharpon!",
        });

        _client.Initialize();
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
