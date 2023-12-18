using Grpc.Net.Client;

public class GrpcChannelService
{
    private static GrpcChannel? channel;
    private static string? grpcEndpoint;
    private static readonly object lockObj = new object();

    private const string DefaultGrpcEndpoint = "http://localhost:8087";

    public static void SetGrpcEndpoint(string endpoint)
    {
        lock (lockObj)
        {
            if (channel == null)
            {
                grpcEndpoint = endpoint;
            }
        }
    }

    public static GrpcChannel GetChannel()
    {
        if (channel == null)
        {
            lock (lockObj)
            {
                if (channel == null)
                {
                    var endpoint = grpcEndpoint ?? Environment.GetEnvironmentVariable(Constants.GrpcEndpointEnvVar) ?? DefaultGrpcEndpoint;
                    channel = GrpcChannel.ForAddress(endpoint);
                }
            }
        }

        return channel;
    }
}
