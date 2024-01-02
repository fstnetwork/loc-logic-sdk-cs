using Saffron.Runtime;
using Saffron.Execution;

public static class Primitive
{
    public async static Task<TaskPayload> FetchPayload()
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var resp = await client.FetchPayloadAsync(new FetchPayloadRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
        });

        return resp.TaskPayload;
    }

    public async static Task<Saffron.Execution.Task> FetchTask()
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var resp = await client.FetchTaskAsync(new FetchTaskRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
        });

        return resp.Task;
    }
}
