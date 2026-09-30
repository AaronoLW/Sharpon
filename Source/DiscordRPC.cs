using DiscordRPC;

public class RichPresence
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
}
