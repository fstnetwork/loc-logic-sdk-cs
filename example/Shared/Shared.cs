namespace Utils;

public static class CustomAgent
{
    public static async Task SimpleLog(string message)
    {
        await LoggingAgent.Log("INFO", "安安你好哇");
    }
}
