public abstract class Payload { }

public class HttpPayload : Payload
{
    public IdentityContext ApiGatewayIdentityContext { get; set; }
    public IdentityContext ApiRouteIdentityContext { get; set; }
    public Peer Destination { get; set; }
    public HttpRequest Request { get; set; }
    public string RequestId { get; set; }
    public Peer Source { get; set; }
}

public class MessagePayload : Payload
{
    public IdentityContext ClientIdentityContext { get; set; }
    public byte[] Data { get; set; }
    public Subscriber Subscriber { get; set; }
}

public class EventPayload : Payload { }

public class Peer
{
    public Address Address { get; set; }
}

public abstract class Address
{

}

public class SocketAddress : Address
{
    public string Address { get; set; }
    public string Protocol { get; set; }
}

public class Pipe : Address
{
    public int Mode { get; set; }
    public string Path { get; set; }
}

public class HttpRequest
{
    public byte[] Data { get; set; }
    public Dictionary<string, string> Headers { get; set; }
    public string Host { get; set; }
    public string Method { get; set; }
    public string Path { get; set; }
    public string Query { get; set; }
    public string Scheme { get; set; }
    public string Version { get; set; }
}

public class Subscriber
{
    public KafkaSubscriber Kafka { get; set; }
}

public class KafkaSubscriber
{
    public List<string> Brokers { get; set; }
    public string GroupId { get; set; }
    public long Offset { get; set; }
    public int Partition { get; set; }
    public string Topic { get; set; }
}