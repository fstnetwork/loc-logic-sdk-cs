extern alias Shared;

public static class GenericLogic
{
    public static async Task run(GenericContext ctx)
    {
        Console.WriteLine("[START LOGIC]");

        await LoggingAgent.Log("INFO", "安安你好哇");
        await Shared.Utils.CustomAgent.SimpleLog("安安你好哇");

        var db = await DatabaseAgent.Acquire("Agent-Configuration");
        Console.WriteLine($"Reply from DatabaseAgent: {db}");

        errorTrace(2);

        Console.WriteLine("[END LOGIC]");
    }

    public static async Task handleError(GenericContext ctx, Exception error)
    {
        Console.WriteLine("[HANDLE ERROR]");
        await LoggingAgent.Log("ERROR", $"Error occurred: {error}");
    }

    private static void errorTrace(int n)
    {
        if (n == 0) throw new Exception("BOMB!");
        errorTrace(n - 1);
    }
}
