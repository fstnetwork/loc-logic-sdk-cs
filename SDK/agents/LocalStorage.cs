using Google.Protobuf;
using Saffron.Runtime;

public static class LocalStorageAgent
{
    public async static Task<StorageValue> Get(string key)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var resp = await client.LocalStorageGetAsync(new LocalStorageGetRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            Key = key,
        });

        return new StorageValue(resp.Value);
    }

    public async static Task Put(string key, StorageValue value, ulong? timeoutSeconds)
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

public class StorageValue
{
    public enum ValueType
    {
        None = 0,
        Bytes = 1,
        String = 2,
        Json = 3
    }

    public ValueType Type { get; private set; } = ValueType.None;
    public byte[]? BytesValue
    {
        get => Type == ValueType.Bytes ? this.BytesValue : null;
        set
        {
            Type = ValueType.Bytes;
            this.BytesValue = value;
        }
    }
    public string? StringValue
    {
        get => Type == ValueType.String ? this.StringValue : null;
        set
        {
            Type = ValueType.String;
            this.StringValue = value;
        }
    }
    public object? JsonValue
    {
        get => Type == ValueType.Json ? this.JsonValue : null;
        set
        {
            Type = ValueType.Json;
            this.JsonValue = value;
        }
    }

    public StorageValue(Saffron.Runtime.StorageValue storageValue)
    {
        switch (storageValue.ValueCase)
        {
            case Saffron.Runtime.StorageValue.ValueOneofCase.BytesValue:
                Type = ValueType.Bytes;
                this.BytesValue = storageValue.BytesValue.ToByteArray();
                break;
            case Saffron.Runtime.StorageValue.ValueOneofCase.StringValue:
                Type = ValueType.String;
                this.StringValue = storageValue.StringValue;
                break;
            case Saffron.Runtime.StorageValue.ValueOneofCase.JsonValue:
                Type = ValueType.Json;
                this.JsonValue = storageValue.JsonValue;
                break;
        }
    }

    public Saffron.Runtime.StorageValue ToProto()
    {
        switch (Type)
        {
            case ValueType.Bytes:
                return new Saffron.Runtime.StorageValue
                {
                    BytesValue = ByteString.CopyFrom(this.BytesValue)
                };

            case ValueType.String:
                return new Saffron.Runtime.StorageValue
                {
                    StringValue = this.StringValue
                };

            case ValueType.Json:
                return new Saffron.Runtime.StorageValue
                {
                    JsonValue = Utils.ConvertObjectToValue(this.JsonValue)
                };

            default:
                throw new Exception("Invalid StorageValue type");
        }
    }
}
