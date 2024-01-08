using Saffron.Runtime;

public class Context : AbstractContext { }

public abstract class AbstractContext
{
    protected Payload? payload;
    protected LogicTask? task;

    public async Task<Payload> GetPayload()
    {
        if (payload == null)
        {
            payload = await FetchPayload();
        }
        return payload;
    }

    public async Task<LogicTask> GetTask()
    {
        if (task == null)
        {
            task = await FetchTask();
        }
        return task;
    }

    private async Task<Payload> FetchPayload()
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var req = new FetchPayloadRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
        };
        var resp = await client.FetchPayloadAsync(req);

        return new Payload(resp.TaskPayload);
    }

    private async Task<LogicTask> FetchTask()
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var req = new FetchTaskRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
        };
        var resp = await client.FetchTaskAsync(req);

        return new LogicTask(resp.Task);
    }
}

