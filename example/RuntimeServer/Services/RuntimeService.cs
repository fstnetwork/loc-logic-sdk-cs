using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

namespace RuntimeServer.Services;

public class RuntimeService : Runtime.RuntimeBase
{
    private bool isRailwayOk { get; set; } = true;
    private RailwayError? error { get; set; } = null;

    public override Task<BoolValue> IsRailwayOk(IsRailwayOkRequest request, ServerCallContext context)
    {
        return Task.FromResult(new BoolValue
        {
            Value = this.isRailwayOk,
        });
    }
    
    public override Task<Empty> SwitchRailway(RailwaySwitchRequest request, ServerCallContext context)
    {
        this.isRailwayOk = false;
        this.error = new RailwayError
        {
            Name = request.Name,
            Message = request.Message,
        };

        return Task.FromResult(new Empty());
    }

    public override Task<GetRailwayErrorResponse> GetRailwayError(GetRailwayErrorRequest request, ServerCallContext context)
    {
        return Task.FromResult(new GetRailwayErrorResponse
        {
            Error = this.error,
        });
    }

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
