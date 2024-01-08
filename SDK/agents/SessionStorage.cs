using Saffron.Runtime;

public static class SessionStorageAgent
{
    public async static Task<StorageValue?> Get(string key)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var resp = await client.SessionStorageGetAsync(new SessionStorageGetRequest
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

    public async static Task Put(string key, StorageValue value)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        await client.SessionStoragePutAsync(new SessionStoragePutRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            Key = key,
            Value = value.ToProto(),
        });
    }

    public async static Task Delete(string key)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        await client.SessionStorageDeleteAsync(new SessionStorageDeleteRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            Key = key,
        });
    }
}
