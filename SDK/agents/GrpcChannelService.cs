using Grpc.Net.Client;

public class GrpcChannelService
{
    private static GrpcChannel? channel;
    private static string? grpcEndpoint;
    private static readonly object lockObj = new object();

    private const string DefaultGrpcEndpoint = "http://localhost:5224";

    public static void SetGrpcEndpoint(string endpoint)
    {
        lock (lockObj)
        {
            if (channel == null)
            {
                grpcEndpoint = endpoint;
            }
            else
            {
                throw new InvalidOperationException("Cannot set the gRPC endpoint after the channel has been created.");
            }
        }
    }

    // static GrpcChannelService()
    // {
    //     var grpcEndpoint = Environment.GetEnvironmentVariable(GrpcEndpointEnvVar) ?? DefaultGrpcEndpoint;
    //     channel = GrpcChannel.ForAddress(grpcEndpoint);
    // }


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

    // public static GrpcChannel GetChannel() => channel;
}
