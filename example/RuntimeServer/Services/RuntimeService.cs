using Grpc.Core;
using Google.Protobuf.WellKnownTypes;

namespace Saffron.Runtime.Services;

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

    public override Task<AcquireDatabaseResponse> AcquireDatabase(AcquireDatabaseRequest request, ServerCallContext context)
    {
        Console.WriteLine("Call AcquireDatabase");
        return Task.FromResult(new AcquireDatabaseResponse { });
    }

    public override Task<Empty> ReleaseDatabase(ReleaseDatabaseRequest request, ServerCallContext context)
    {
        Console.WriteLine("Call ReleaseDatabase");
        return Task.FromResult(new Empty());
    }

    public override Task<QueryDatabaseResponse> QueryDatabase(QueryDatabaseRequest request, ServerCallContext context)
    {
        Console.WriteLine("Call QueryDatabase");
        var resp = new QueryDatabaseResponse
        {
            Columns = {
                new DatabaseColumn { Name = "id", Type = "Int4" },
                new DatabaseColumn { Name = "message", Type = "Text" },
                new DatabaseColumn { Name = "timestamp", Type = "Datetimen" },
            },
            Rows = {
                new ListValue {
                    Values = {
                        Value.ForNumber(2),
                        Value.ForString("foo"),
                        Value.ForString("2023-06-19T05:17:29.620Z"),
                    },
                },
                new ListValue {
                    Values = {
                        Value.ForNumber(3),
                        Value.ForString("bar"),
                        Value.ForString("2023-08-21T09:00:00Z"),
                    },
                },
            },
        };
        return Task.FromResult(resp);
    }

    public override Task<ExecuteDatabaseResponse> ExecuteDatabase(ExecuteDatabaseRequest request, ServerCallContext context)
    {
        Console.WriteLine("Call ExecuteDatabase");
        var resp = new ExecuteDatabaseResponse
        {
            AffectedRows = 1,
        };
        return Task.FromResult(resp);
    }

    public override Task<Empty> BeginTransaction(BeginTransactionRequest request, ServerCallContext context)
    {
        Console.WriteLine("Call BeginTransaction");
        return Task.FromResult(new Empty());
    }

    public override Task<Empty> CommitTransaction(CommitTransactionRequest request, ServerCallContext context)
    {
        Console.WriteLine("Call CommitTransaction");
        return Task.FromResult(new Empty());
    }

    public override Task<Empty> RollbackTransaction(RollbackTransactionRequest request, ServerCallContext context)
    {
        Console.WriteLine("Call RollbackTransaction");
        return Task.FromResult(new Empty());
    }

    // Result Agent
    public override Task<Empty> SetResult(SetResultRequest request, ServerCallContext context)
    {
        Console.WriteLine($"Call SetResult: {request.Result}");
        return Task.FromResult(new Empty());
    }
}
