using Google.Protobuf;

public static class Constants
{
    public const string GrpcEndpointEnvVar = "SAFFRON_RUNTIME_GRPC_ENDPOINT";
}

public static class Global
{
    private static TaskKey? _taskKey;

    public static TaskKey TaskKey
    {
        get
        {
            if (_taskKey == null)
            {
                throw new InvalidOperationException("Global variable 'TaskKey' has not been initialized.");
            }
            return _taskKey;
        }
        set
        {
            _taskKey = value;
        }
    }
}
