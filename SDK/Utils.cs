using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;

public static class Utils
{
    public static JObject ConvertStructToJson(Struct gRPCStruct)
    {
        string jsonString = JsonFormatter.Default.Format(gRPCStruct);
        return JObject.Parse(jsonString);
    }

    public static Value ConvertObjectToValue(object obj)
    {
        var value = new Value();

        switch (obj)
        {
            case null:
                value.NullValue = NullValue.NullValue;
                break;
            case int i:
                value.NumberValue = i;
                break;
            case long l:
                value.NumberValue = l;
                break;
            case float f:
                value.NumberValue = f;
                break;
            case double d:
                value.NumberValue = d;
                break;
            case bool b:
                value.BoolValue = b;
                break;
            case string s:
                value.StringValue = s;
                break;
            default:
                throw new ArgumentException("Unsupported object type");
        }

        return value;
    }

    public static byte[] ConvertToByteArray<T>(T gRPCStruct)
        where T : IMessage<T>
    {
        using (var ms = new MemoryStream())
        {
            gRPCStruct.WriteTo(ms);
            return ms.ToArray();
        }
    }

    public static T? ToObject<T>(this byte[] buf)
        where T : IMessage<T>, new()
    {
        if (buf == null)
            return default;

        using (MemoryStream ms = new MemoryStream())
        {
            ms.Write(buf, 0, buf.Length);
            ms.Seek(0, SeekOrigin.Begin);

            MessageParser<T> parser = new MessageParser<T>(() => new T());
            return parser.ParseFrom(ms);
        }
    }

    public static UInt128 BytesToUInt128(byte[] bytes)
    {
        if (bytes.Length != 16)
        {
            throw new ArgumentException("Byte array must be exactly 16 bytes long.");
        }

        if (BitConverter.IsLittleEndian)
        {
            Array.Reverse(bytes);
        }
        ulong upper = BitConverter.ToUInt64(bytes, 8);
        ulong lower = BitConverter.ToUInt64(bytes, 0);

        return new UInt128(upper, lower);
    }

    public static byte[] UInt128ToBytes(UInt128 value)
    {
        byte[] bytes = new byte[16];
        for (int i = 15; i >= 0; i--)
        {
            bytes[i] = (byte)(value % 256);
            value /= 256;
        }
        return bytes;
    }

    public static string EncodeUint128(UInt128 value)
    {
        var bytes = UInt128ToBytes(value);
        string base64 = Convert.ToBase64String(bytes);
        return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }
}
