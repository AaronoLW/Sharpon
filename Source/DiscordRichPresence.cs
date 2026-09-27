using DiscordRPC;

public class DiscordRichPresence : IDisposable
{
    private readonly DiscordRpcClient _client;

    public DiscordRichPresence()
    {
        _client = new("1441930534052827267");
        _client.Initialize();

        _client.SetPresence(new()
        {
            Details = "Sharpon",
        });
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
