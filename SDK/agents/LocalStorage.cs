using Google.Protobuf;
using Saffron.Runtime;

public static class LocalStorageAgent
{
    public async static Task<StorageValue?> Get(string key)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var resp = await client.LocalStorageGetAsync(new LocalStorageGetRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            Key = key,
        });

        if (resp.Value == null)
        {
            return null;
        }
        return new StorageValue(resp.Value);
    }

    public async static Task Put(string key, StorageValue value, ulong? timeoutSeconds = null)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var req = new LocalStoragePutRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            Key = key,
            Value = value.ToProto(),
        };
        if (timeoutSeconds.HasValue)
        {
            req.TimeoutSeconds = timeoutSeconds.Value;
        }
        await client.LocalStoragePutAsync(req);
    }

    public async static Task Delete(string key)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        await client.LocalStorageDeleteAsync(new LocalStorageDeleteRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            Key = key,
        });
    }
}
