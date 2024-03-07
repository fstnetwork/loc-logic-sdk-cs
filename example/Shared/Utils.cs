namespace Utils;

public static class CustomLoggingAgent
{
    public static async Task SimpleLog(string message)
    {
        await LoggingAgent.Log("INFO", $"CustomLoggingAgent Says: {message}");
    }
}
