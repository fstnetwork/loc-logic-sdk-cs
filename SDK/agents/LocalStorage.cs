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

public class StorageValue
{
    public enum ValueType
    {
        None = 0,
        Bytes = 1,
        String = 2,
        Json = 3
    }

    public byte[]? BytesValue { get; set; }
    public string? StringValue { get; set; }
    public object? JsonValue { get; set; }

    public StorageValue() { }

    public static StorageValue FromBytes(byte[] bytes)
    {
        return new StorageValue
        {
            BytesValue = bytes
        };
    }

    public static StorageValue FromString(string str)
    {
        return new StorageValue
        {
            StringValue = str
        };
    }

    public static StorageValue FromJson(object obj)
    {
        return new StorageValue
        {
            JsonValue = obj
        };
    }

    public StorageValue(Saffron.Runtime.StorageValue storageValue)
    {
        switch (storageValue.ValueCase)
        {
            case Saffron.Runtime.StorageValue.ValueOneofCase.BytesValue:
                this.BytesValue = storageValue.BytesValue.ToByteArray();
                break;
            case Saffron.Runtime.StorageValue.ValueOneofCase.StringValue:
                this.StringValue = storageValue.StringValue;
                break;
            case Saffron.Runtime.StorageValue.ValueOneofCase.JsonValue:
                this.JsonValue = storageValue.JsonValue;
                break;
        }
    }

    public Saffron.Runtime.StorageValue ToProto()
    {
        if (BytesValue != null)
        {
            return new Saffron.Runtime.StorageValue
            {
                BytesValue = ByteString.CopyFrom(BytesValue)
            };
        }

        if (StringValue != null)
        {
            return new Saffron.Runtime.StorageValue
            {
                StringValue = StringValue
            };
        }

        if (JsonValue != null)
        {
            return new Saffron.Runtime.StorageValue
            {
                JsonValue = Utils.ConvertObjectToValue(JsonValue)
            };
        }

        throw new Exception("Invalid StorageValue type");
    }

    public ValueType GetValueType()
    {
        if (BytesValue != null)
        {
            return ValueType.Bytes;
        }

        if (StringValue != null)
        {
            return ValueType.String;
        }

        if (JsonValue != null)
        {
            return ValueType.Json;
        }

        return ValueType.None;
    }
}
