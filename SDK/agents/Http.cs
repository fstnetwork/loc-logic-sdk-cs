using System.Net;
using Saffron.Runtime;
using System.Net.Http.Headers;
using Google.Protobuf;

public static class HttpAgent
{
    public async static Task<HttpClient> Acquire(string name)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var resp = await client.AcquireHttpAsync(new AcquireHttpRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            Name = name,
        });

        return new HttpClient(resp.AgentConfigurationId);
    }
}

public class HttpClient
{
    readonly Saffron.Common.Uuid AgentConfigurationId;

    public HttpClient(Saffron.Common.Uuid agentConfigurationId)
    {
        this.AgentConfigurationId = agentConfigurationId;
    }

    public async Task<HttpResponseMessage> Send(SendHttpRequest request)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var resp = await client.SendHttpAsync(await request.ToProto(AgentConfigurationId));

        return ToHttpResponseMessage(resp);
    }

    public async Task<HttpResponseMessage> Send(string path)
    {
        return await Send(new SendHttpRequest(path));
    }

    public async Task<HttpResponseMessage> Send(HttpMethod method, SendHttpRequest request)
    {
        request.Method = method;
        return await Send(request);
    }

    public async Task<HttpResponseMessage> Send(HttpMethod method, string path)
    {
        var request = new SendHttpRequest(path)
        {
            Method = method
        };
        return await Send(request);
    }

    public async Task<HttpResponseMessage> Get(string path)
    {
        return await Send(HttpMethod.Get, path);
    }

    public async Task<HttpResponseMessage> Post(string path)
    {
        return await Send(HttpMethod.Post, path);
    }

    public async Task<HttpResponseMessage> Post(SendHttpRequest request)
    {
        return await Send(HttpMethod.Post, request);
    }

    public async Task<HttpResponseMessage> Put(string path)
    {
        return await Send(HttpMethod.Put, path);
    }

    public async Task<HttpResponseMessage> Put(SendHttpRequest request)
    {
        return await Send(HttpMethod.Put, request);
    }

    public async Task<HttpResponseMessage> Patch(string path)
    {
        return await Send(HttpMethod.Patch, path);
    }

    public async Task<HttpResponseMessage> Patch(SendHttpRequest request)
    {
        return await Send(HttpMethod.Patch, request);
    }

    public async Task<HttpResponseMessage> Delete(string path)
    {
        return await Send(HttpMethod.Delete, path);
    }

    public async Task<HttpResponseMessage> Delete(SendHttpRequest request)
    {
        return await Send(HttpMethod.Delete, request);
    }

    private static HttpResponseMessage ToHttpResponseMessage(SendHttpResponse proto)
    {
        var response = new HttpResponseMessage((HttpStatusCode)proto.Status)
        {
            Content = new ByteArrayContent(proto.BodyStream.ToByteArray()),
        };

        foreach (var header in proto.Headers)
        {
            if (IsContentHeader(header.Key))
            {
                response.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
            else
            {
                response.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return response;
    }

    private static bool IsContentHeader(string headerName)
    {
        var contentHeaders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Content-Type",
            "Content-Length",
            "Content-Disposition",
        };

        return contentHeaders.Contains(headerName);
    }
}

public class SendHttpRequest
{
    public string Path { get; set; }
    public HttpMethod Method { get; set; } = HttpMethod.Get;
    public HttpContentHeaders Headers { get; set; } = new StringContent("").Headers;
    private Stream _bodyStream = new MemoryStream();
    public Stream BodyStream
    {
        get => _bodyStream;
        set
        {
            _bodyStream = value;
            BodyLength = (ulong)(_bodyStream?.Length ?? 0);
        }
    }
    public ulong BodyLength { get; private set; } = 0;

    public SendHttpRequest(string path)
    {
        this.Path = path;
    }

    public SendHttpRequest(string path, HttpContent content)
    {
        this.Path = path;
        this.Headers = content.Headers;
        this.BodyStream = content.ReadAsStream();
    }

    public async Task<Saffron.Runtime.SendHttpRequest> ToProto(Saffron.Common.Uuid configurationId)
    {
        var request = new Saffron.Runtime.SendHttpRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            ConfigurationId = configurationId,
            Path = this.Path,
            Method = this.Method.ToString(),
            Body = await ByteString.FromStreamAsync(this.BodyStream),
            BodyLength = this.BodyLength,
        };
        foreach (var header in this.Headers)
        {
            request.Headers.Add(header.Key, header.Value.FirstOrDefault());
        }

        return request;
    }
}
