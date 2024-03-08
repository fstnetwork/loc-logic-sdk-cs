using Google.Protobuf;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

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
    public JsonNode? JsonValue { get; set; }

    public StorageValue() { }

    public override string ToString()
    {
        if (BytesValue != null)
        {
            return $"[Bytes] {BytesValue}";
        }

        if (StringValue != null)
        {
            return $"[String] {StringValue}";
        }

        if (JsonValue != null)
        {
            var jsonString = JsonSerializer.Serialize(JsonValue, JsonNodeSourceGenerationContext.Default.JsonNode);
            return $"[Json] {jsonString}";
        }

        return "Invalid StorageValue type";
    }

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

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    [RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    public static StorageValue FromJson(object obj)
    {
        string jsonString = JsonSerializer.Serialize(obj);
        JsonNode? jsonNode = JsonNode.Parse(jsonString);

        return new StorageValue
        {
            JsonValue = jsonNode
        };
    }

    /// <summary>
    /// Accept a `JsonTypeInfo<T>` to serialize the object to JSON in native AOT compilation.
    /// For more information, see https://learn.microsoft.com/zh-tw/dotnet/standard/serialization/system-text-json/source-generation?pivots=dotnet-7-0
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="jsonTypeInfo"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static StorageValue FromJson<T>(T obj, JsonTypeInfo<T> jsonTypeInfo)
    {
        string jsonString = JsonSerializer.Serialize(obj, jsonTypeInfo);
        JsonNode? jsonNode = JsonNode.Parse(jsonString);

        return new StorageValue
        {
            JsonValue = jsonNode
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
                this.JsonValue = Utils.ConvertValueToJson(storageValue.JsonValue);
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
