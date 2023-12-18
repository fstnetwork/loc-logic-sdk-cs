using RuntimeServer;

public static class LoggingAgent
{
    public async static Task Trace(string message)
    {
        await Log("TRACE", message);
    }

    public async static Task Debug(string message)
    {
        await Log("DEBUG", message);
    }

    public async static Task Info(string message)
    {
        await Log("INFO", message);
    }

    public async static Task Warn(string message)
    {
        await Log("WARN", message);
    }

    public async static Task Error(string message)
    {
        await Log("ERROR", message);
    }

    public async static Task Log(string level, string message)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        await client.LogAsync(new LogRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            Level = level,
            Message = message
        });
    }
}
