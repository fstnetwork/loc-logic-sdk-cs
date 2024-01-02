using Saffron.Execution;

public abstract class Payload { }

public class HttpPayload : Payload
{
    public IdentityContext ApiGatewayIdentityContext { get; set; }
    public IdentityContext ApiRouteIdentityContext { get; set; }
    public Peer Destination { get; set; }
    public HttpRequest Request { get; set; }
    public string RequestId { get; set; }
    public Peer Source { get; set; }

    public HttpPayload(TaskPayload.Types.HttpPayload http)
    {
        ApiGatewayIdentityContext = new IdentityContext(http.ApiGatewayIdentityContext);
        ApiRouteIdentityContext = new IdentityContext(http.ApiRouteIdentityContext);
        Destination = new Peer(http.Destination);
        Request = new HttpRequest(http.Request);
        RequestId = http.RequestId;
        Source = new Peer(http.Source);
    }
}

public class MessagePayload : Payload
{
    public IdentityContext ClientIdentityContext { get; set; }
    public byte[] Data { get; set; }
    public Subscriber Subscriber { get; set; }

    public MessagePayload(TaskPayload.Types.MessagePayload message)
    {
        switch (message.Data.DataCase)
        {
            case TaskPayload.Types.Data.DataOneofCase.Bytes:
                Data = message.Data.Bytes.ToByteArray();
                break;

            default:
                throw new InvalidOperationException("Unknown or unsupported body type");
        }

        ClientIdentityContext = new IdentityContext(message.ClientIdentityContext);
        Subscriber = new Subscriber(message.Subscriber);
    }
}

public class EventPayload : Payload { }

public class Peer
{
    public Address Address { get; set; }

    public Peer(TaskPayload.Types.HttpPayload.Types.Peer peer)
    {
        switch (peer.Address.AddressCase)
        {
            case TaskPayload.Types.HttpPayload.Types.Address.AddressOneofCase.SocketAddress:
                this.Address = new SocketAddress(peer.Address.SocketAddress);
                break;

            case TaskPayload.Types.HttpPayload.Types.Address.AddressOneofCase.Pipe:
                this.Address = new Pipe(peer.Address.Pipe);
                break;

            default:
                throw new InvalidOperationException("Unknown address type");
        }
    }
}

public abstract class Address { }

public class SocketAddress : Address
{
    public string Protocol { get; set; }
    public byte[] Address { get; set; }
    public uint Port { get; set; }

    public SocketAddress(TaskPayload.Types.HttpPayload.Types.SocketAddress address)
    {
        Protocol = address.Protocol.ToString();
        Address = address.Address.ToByteArray();
        Port = address.Port;
    }
}

public class Pipe : Address
{
    public string Path { get; set; }
    public uint Mode { get; set; }

    public Pipe(TaskPayload.Types.HttpPayload.Types.Pipe pipe)
    {
        Path = pipe.Path;
        Mode = pipe.Mode;
    }
}

public class HttpRequest
{
    public byte[] Data { get; set; }
    public Dictionary<byte[], byte[]> Headers { get; set; }
    public string Host { get; set; }
    public string Method { get; set; }
    public string Path { get; set; }
    public string Query { get; set; }
    public string Scheme { get; set; }
    public string Version { get; set; }

    public HttpRequest(TaskPayload.Types.HttpPayload.Types.HttpRequest httpRequest)
    {
        switch (httpRequest.Body.DataCase)
        {
            case TaskPayload.Types.Data.DataOneofCase.Bytes:
                Data = httpRequest.Body.Bytes.ToByteArray();
                break;

            default:
                throw new InvalidOperationException("Unknown or unsupported body type");
        }

        Headers = httpRequest.Headers.ToDictionary(x => x.Name.ToByteArray(), x => x.Value.ToByteArray());
        Host = httpRequest.Host;
        Method = httpRequest.Method;
        Path = httpRequest.Path;
        Query = httpRequest.Query;
        Scheme = httpRequest.Scheme;
        Version = httpRequest.Version.ToString();
    }
}

public class Subscriber
{
    public KafkaSubscriber Kafka { get; set; }

    public Subscriber(TaskPayload.Types.MessagePayload.Types.Subscriber subscriber)
    {
        this.Kafka = new KafkaSubscriber(subscriber.Kafka);
    }
}

public class KafkaSubscriber
{
    public List<string> Brokers { get; set; }
    public string GroupId { get; set; }
    public long Offset { get; set; }
    public int Partition { get; set; }
    public string Topic { get; set; }

    public KafkaSubscriber(TaskPayload.Types.MessagePayload.Types.KafkaSubscriber kafkaSubscriber)
    {
        this.Brokers = kafkaSubscriber.Brokers.ToList();
        this.GroupId = kafkaSubscriber.GroupId;
        this.Offset = kafkaSubscriber.Offset;
        this.Partition = kafkaSubscriber.Partition;
        this.Topic = kafkaSubscriber.Topic;
    }
}