using Saffron.Runtime;

public static class Railway
{
    public async static Task<bool> IsRailwayOk()
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var resp = await client.IsRailwayOkAsync(new IsRailwayOkRequest
        {
            TaskKey = Global.TaskKey.ToProto()
        });

        return resp.Value;
    }

    public async static Task SwitchRailway(string name, string message)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        await client.SwitchRailwayAsync(new RailwaySwitchRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            Name = name,
            Message = message,
        });
    }

    public async static Task<RailwayError?> GetRailwayError()
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var resp = await client.GetRailwayErrorAsync(new GetRailwayErrorRequest
        {
            TaskKey = Global.TaskKey.ToProto()
        });
        if (resp.Error == null)
        {
            return null;
        }

        return new RailwayError(resp.Error);
    }
}
