using Grpc.Core;
using RuntimeServer;
using Google.Protobuf.WellKnownTypes;

namespace RuntimeServer.Services;

public class RuntimeService : Runtime.RuntimeBase
{
    public override Task<Empty> Log(LogRequest request, ServerCallContext context)
    {
        Console.WriteLine($"[{request.Level}] {request.Message}");

        return Task.FromResult(new Empty());
    }

    public override Task<AcquireResponse> Acquire(AcquireRequest request, ServerCallContext context)
    {
        Console.WriteLine("Call Acquire");
        return Task.FromResult(new AcquireResponse { });
    }
}
