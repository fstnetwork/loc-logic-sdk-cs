using Saffron.Runtime;

public static class LogicVariable
{
    public async static Task<string?> GetLogicVariable(string name)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var resp = await client.GetLogicVariableAsync(new GetLogicVariableRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            Name = name,
        });

        return resp.Value;
    }
}
