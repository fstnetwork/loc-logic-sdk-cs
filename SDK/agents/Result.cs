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

    public async static Task SetHttpStatusCode(UInt16 statusCode)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        await client.SetHttpStatusCodeAsync(new SetHttpStatusCodeRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            StatusCode = statusCode,
        });
    }
}
