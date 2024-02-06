using Saffron.Runtime;

public static class LoggingAgent
{
    public async static Task Trace(string message)
    {
        await Log("Trace", message);
    }

    public async static Task Trace(object message)
    {
        await Log("Trace", message);
    }

    public async static Task Debug(string message)
    {
        await Log("Debug", message);
    }

    public async static Task Debug(object message)
    {
        await Log("Debug", message);
    }

    public async static Task Info(string message)
    {
        await Log("Info", message);
    }

    public async static Task Info(object message)
    {
        await Log("Info", message);
    }

    public async static Task Warn(string message)
    {
        await Log("Warn", message);
    }

    public async static Task Warn(object message)
    {
        await Log("Warn", message);
    }

    public async static Task Error(string message)
    {
        await Log("Error", message);
    }

    public async static Task Error(object message)
    {
        await Log("Error", message);
    }

    public async static Task Log(string level, string message)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        await client.LogAsync(new LogRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            Level = level,
            Plaintext = message,
        });
    }

    public async static Task Log(string level, object message)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        await client.LogAsync(new LogRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            Level = level,
            Json = Utils.ConvertObjectToValue(message),
        });
    }
}
