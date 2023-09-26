using Google.Protobuf.WellKnownTypes;
using RuntimeServer;

public class GenericContext : AbstractContext
{
    public GenericContext(string str)
    {
        // TODO:
    }
}

public class AggregatorContext : AbstractContext
{
    // TODO:
}

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

    public LogicTask GetTask()
    {
        if (task == null)
        {
            task = FetchTask();
        }
        return task;
    }

    private async Task<Payload> FetchPayload()
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var payload = await client.FetchPayloadAsync(new Empty());

        // TODO: implement payload deserialization
        return new HttpPayload { };
    }

    private LogicTask FetchTask()
    {
        return new LogicTask { };
    }
}

