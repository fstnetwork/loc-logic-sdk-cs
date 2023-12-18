using Saffron.Runtime;
using Saffron.Execution;

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

        var req = new FetchPayloadRequest();
        var resp = await client.FetchPayloadAsync(req);

        switch (resp.TaskPayload.PayloadCase)
        {
            case TaskPayload.PayloadOneofCase.Http:
                return new HttpPayload(resp.TaskPayload.Http);

            case TaskPayload.PayloadOneofCase.Event:
                return new EventPayload { };

            case TaskPayload.PayloadOneofCase.Message:
                return new MessagePayload(resp.TaskPayload.Message);

            default:
                throw new Exception("Unknown payload type");
        }
    }

    private async Task<LogicTask> FetchTask()
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var req = new FetchTaskRequest();
        var resp = await client.FetchTaskAsync(req);

        return new LogicTask(resp.Task);
    }
}

