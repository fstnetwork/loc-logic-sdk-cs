using Grpc.Core;
using Google.Protobuf.WellKnownTypes;
using Saffron.Common;

namespace Saffron.Runtime.Services;

public class RuntimeService : Runtime.RuntimeBase
{
    private bool isRailwayOk { get; set; } = true;
    private RailwayError? error { get; set; } = null;

    // Primitive
    public override Task<FetchPayloadResponse> FetchPayload(FetchPayloadRequest request, ServerCallContext context)
    {
        return Task.FromResult(new FetchPayloadResponse
        {
            TaskPayload = new Execution.TaskPayload
            {
                Http = new Execution.TaskPayload.Types.HttpPayload
                {
                    ApiGatewayIdentityContext = new Common.NonVersionedIdentityContext
                    {
                        Name = "fake-api-gateway",
                        Id = Utils.ZeroUuid(),
                    },
                    ApiRouteIdentityContext = new Common.NonVersionedIdentityContext
                    {
                        Name = "fake-api-route",
                        Id = Utils.ZeroUuid(),
                    },
                    Source = new Execution.TaskPayload.Types.HttpPayload.Types.Peer
                    {
                        Address = new Execution.TaskPayload.Types.HttpPayload.Types.Address
                        {
                            SocketAddress = new Execution.TaskPayload.Types.HttpPayload.Types.SocketAddress
                            {
                                Protocol = Execution.TaskPayload.Types.HttpPayload.Types.Protocol.Tcp,
                                Address = Google.Protobuf.ByteString.CopyFrom(new byte[] { 127, 0, 0, 1 }),
                                Port = 8080,
                            }
                        }
                    },
                    Destination = new Execution.TaskPayload.Types.HttpPayload.Types.Peer
                    {
                        Address = new Execution.TaskPayload.Types.HttpPayload.Types.Address
                        {
                            SocketAddress = new Execution.TaskPayload.Types.HttpPayload.Types.SocketAddress
                            {
                                Protocol = Execution.TaskPayload.Types.HttpPayload.Types.Protocol.Tcp,
                                Address = Google.Protobuf.ByteString.CopyFrom(new byte[] { 127, 0, 0, 1 }),
                                Port = 8200,
                            }
                        }
                    },
                    RequestId = "fake-request-id",
                    Request = new Execution.TaskPayload.Types.HttpPayload.Types.HttpRequest
                    {
                        Scheme = "http",
                        Method = "GET",
                        Path = "/api/v1/test-external-runtime",
                        Query = "foo=bar&baz=qux",
                        Version = Execution.TaskPayload.Types.HttpPayload.Types.Version.Http11,
                        Headers =
                        {
                            new Execution.TaskPayload.Types.HttpPayload.Types.HttpRequest.Types.Header
                            {
                                Name = Google.Protobuf.ByteString.CopyFromUtf8("Content-Type"),
                                Value = Google.Protobuf.ByteString.CopyFromUtf8("application/json"),
                            },
                        },
                        Host = "localhost",
                        Body = new Execution.TaskPayload.Types.Data
                        {
                            Bytes = Google.Protobuf.ByteString.CopyFromUtf8(@"{
                                ""foo"": ""bar"",
                                ""baz"": ""qux"",
                            }"),
                        },
                    },
                },
            },
        });
    }

    public override Task<FetchTaskResponse> FetchTask(FetchTaskRequest request, ServerCallContext context)
    {
        return Task.FromResult(new FetchTaskResponse
        {
            Task = new Saffron.Execution.Task
            {
                TaskKey = new Saffron.Execution.TaskKey
                {
                    ExecutionId = Utils.ZeroU128(),
                    TaskId = Utils.ZeroU128(),
                },
                StartTimestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                DataProcess = new Saffron.Common.VersionedIdentityContext
                {
                    Name = "fake-data-process",
                    Id = new Saffron.Common.VersionedIdentity
                    {
                        PermanentIdentity = Utils.ZeroUuid(),
                        Revision = Utils.Revision(1),
                    },
                },
                CurrentLogic = new Saffron.Common.VersionedIdentityContext
                {
                    Name = "fake-current-logic",
                    Id = new Saffron.Common.VersionedIdentity
                    {
                        PermanentIdentity = Utils.ZeroUuid(),
                        Revision = Utils.Revision(1),
                    },
                },
                CurrentLogicType = Saffron.Logic.LogicType.Generic,
            },
        });
    }

    // Logic Railway
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

    // Database Agent
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

    // Event Agent
    public override Task<Empty> EmitEvent(EmitEventRequest request, ServerCallContext context)
    {
        Console.WriteLine("Call EmitEvent");
        return Task.FromResult(new Empty());
    }

    public override Task<SearchEventResponse> SearchEvent(SearchEventRequest request, ServerCallContext context)
    {
        Console.WriteLine("Call SearchEvent");
        var resp = new SearchEventResponse
        {
            Took = 10,
            Count = 1,
            Total = 2,
            Events = {
                new Saffron.Event.Event {
                    DataProcessIdentityContext = new Saffron.Common.VersionedIdentityContext
                    {
                        Name = "dataProcessName",
                        Id = new Saffron.Common.VersionedIdentity
                        {
                            PermanentIdentity = Utils.ZeroUuid(),
                            Revision = Utils.Revision(1),
                        },
                    },
                    LogicIdentityContext = new Saffron.Common.VersionedIdentityContext
                    {
                        Name = "logicName",
                        Id = new Saffron.Common.VersionedIdentity
                        {
                            PermanentIdentity = Utils.ZeroUuid(),
                            Revision = Utils.Revision(1),
                        },
                    },
                    ExecutionId = "execution_id",
                    TaskId = "task_id",
                    Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                    SourceDigitalIdentity = "sourceDigitalIdentity",
                    TargetDigitalIdentity = "targetDigitalIdentity",
                    LabelId = "labelId",
                    LabelName = "labelName",
                    Sequence = 1,
                    Type = "default",
                    Meta = "meta",
                },
            },
            Aggregation = new Saffron.Event.AggregationResult
            {
                Buckets = {
                    new Saffron.Event.AggregationResult.Types.Bucket
                    {
                        Key = {},
                        DocCount = 0,
                    },
                },
                After = { },
            },
        };
        return Task.FromResult(resp);
    }

    public override Task<SearchEventWithPatternResponse> SearchEventWithPattern(SearchEventWithPatternRequest request, ServerCallContext context)
    {
        Console.WriteLine("Call SearchEventWithPattern");
        var resp = new SearchEventWithPatternResponse
        {
            Took = 10,
            Count = 1,
            Total = 2,
            Sequences = {
                new Saffron.Runtime.SearchEventWithPatternResponse.Types.SequenceResult {
                    JoinKeys = { "join_key" },
                    Events = {
                        new Saffron.Event.Event {
                            DataProcessIdentityContext = new Saffron.Common.VersionedIdentityContext
                            {
                                Name = "dataProcessName",
                                Id = new Saffron.Common.VersionedIdentity
                                {
                                    PermanentIdentity = Utils.ZeroUuid(),
                                    Revision = Utils.Revision(1),
                                },
                            },
                            LogicIdentityContext = new Saffron.Common.VersionedIdentityContext
                            {
                                Name = "logicName",
                                Id = new Saffron.Common.VersionedIdentity
                                {
                                    PermanentIdentity = Utils.ZeroUuid(),
                                    Revision = Utils.Revision(1),
                                },
                            },
                            ExecutionId = "execution_id",
                            TaskId = "task_id",
                            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow),
                            SourceDigitalIdentity = "sourceDigitalIdentity",
                            TargetDigitalIdentity = "targetDigitalIdentity",
                            LabelId = "labelId",
                            LabelName = "labelName",
                            Sequence = 1,
                            Type = "default",
                            Meta = "meta",
                        },
                    },
                },
            },
        };
        return Task.FromResult(resp);
    }

    // Http Agent
    public override Task<AcquireHttpResponse> AcquireHttp(AcquireHttpRequest request, ServerCallContext context)
    {
        Console.WriteLine("Call AcquireHttp");
        return Task.FromResult(new AcquireHttpResponse {
            AgentConfigurationId = Utils.ZeroUuid(),
        });
    }

    public override Task<SendHttpResponse> SendHttp(SendHttpRequest request, ServerCallContext context)
    {
        Console.WriteLine("Call SendHttp");

        Console.WriteLine($"***** [{request.Method}] {request.Path} *****");
        Console.WriteLine("Headers:");
        foreach (var header in request.Headers)
        {
            Console.WriteLine($"\t{header.Key}={header.Value}");
        }
        Console.WriteLine($"Body:\n\t{ System.Text.Encoding.UTF8.GetString(request.Body.ToByteArray())}");
        Console.WriteLine();

        return Task.FromResult(new SendHttpResponse
        {
            Status = 200,
            Headers = {
                { "Content-Type", "application/json" },
                { "Server", "MockRuntime/0.0.0" },
            },
            Url = $"https://example.com/{request.Path}",
            BodyStream = Google.Protobuf.ByteString.CopyFromUtf8(@"{
                ""foo"": ""bar"",
                ""baz"": ""qux"",
            }"),
            ContentLength = 100,
        });
    }

    // Logging Agent
    public override Task<Empty> Log(LogRequest request, ServerCallContext context)
    {
        Console.WriteLine($"[{request.Level}] {request.Message}");

        return Task.FromResult(new Empty());
    }

    // Result Agent
    public override Task<Empty> SetResult(SetResultRequest request, ServerCallContext context)
    {
        Console.WriteLine($"Call SetResult: {request.Result}");
        return Task.FromResult(new Empty());
    }
}
