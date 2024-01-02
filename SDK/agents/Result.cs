using Saffron.Runtime;

public static class ResultAgent
{
    public async static Task SetResult(object result)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        await client.SetResultAsync(new SetResultRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            Result = Utils.ConvertObjectToValue(result),
        });
    }
}
