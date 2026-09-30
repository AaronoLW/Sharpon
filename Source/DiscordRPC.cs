using DiscordRPC;

public class RichPresence : IDisposable
{
    private readonly DiscordRpcClient _client = new("1441930534052827267");

    public RichPresence()
    {
        _client.SetPresence(new()
        {
            Details = "Sharpon!"
        });

        _client.Initialize();
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}
