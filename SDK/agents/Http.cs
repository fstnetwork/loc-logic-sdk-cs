using Google.Protobuf;
using Saffron.Runtime;

public static class HttpAgent
{
    public async static Task<Guid> Acquire(string name)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var resp = await client.AcquireHttpAsync(new AcquireHttpRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            Name = name,
        });

        return new Guid(resp.AgentConfigurationId.ToByteArray());
    }
}

class HttpClient
{
    Saffron.Common.Uuid AgentConfigurationId;

    public HttpClient(Saffron.Common.Uuid agentConfigurationId)
    {
        this.AgentConfigurationId = agentConfigurationId;
    }

    // TODO: implement HTTP Agent methods
}
