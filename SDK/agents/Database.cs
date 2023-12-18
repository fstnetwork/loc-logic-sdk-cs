using Google.Protobuf.WellKnownTypes;
using RuntimeServer;
using Newtonsoft.Json.Linq;

public static class DatabaseAgent
{
    public async static Task<DatabaseClient> Acquire(string name)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var resp = await client.AcquireAsync(new AcquireRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            Name = name
        });

        return new DatabaseClient(resp.DataSourceId, resp.ConnectionId);
    }
}

public class DatabaseClient
{
    string DataSourceId;
    string ConnectionId;

    public DatabaseClient(string dataSourceId, string connectionId)
    {
        this.DataSourceId = dataSourceId;
        this.ConnectionId = connectionId;
    }

    public async Task Release()
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        await client.ReleaseAsync(new ReleaseRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            DataSourceId = DataSourceId,
            ConnectionId = ConnectionId
        });
    }

    public async Task<QueryResults> Query(string rawSql, List<object> parameters)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var parameterList = new ListValue();
        foreach (var param in parameters)
        {
            parameterList.Values.Add(Utils.ConvertObjectToValue(param));
        }

        var response = await client.QueryAsync(new QueryRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            DataSourceId = DataSourceId,
            ConnectionId = ConnectionId,
            RawSql = rawSql,
            Params = parameterList,
        });

        return QueryResults.FromProtobuf(response);
    }

    public async Task<QueryResults> Execute(string rawSql, List<object> parameters)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var parameterList = new ListValue();
        foreach (var param in parameters)
        {
            parameterList.Values.Add(Utils.ConvertObjectToValue(param));
        }

        var response = await client.QueryAsync(new QueryRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            DataSourceId = DataSourceId,
            ConnectionId = ConnectionId,
            RawSql = rawSql,
            Params = parameterList,
        });

        return QueryResults.FromProtobuf(response);
    }

    public async Task BeginTransaction()
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        await client.BeginTransactionAsync(new BeginTransactionRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            DataSourceId = DataSourceId,
            ConnectionId = ConnectionId
        });
    }

    public async Task CommitTransaction()
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        await client.CommitTransactionAsync(new CommitTransactionRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            DataSourceId = DataSourceId,
            ConnectionId = ConnectionId
        });
    }

    public async Task RollbackTransaction()
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        await client.RollbackTransactionAsync(new RollbackTransactionRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            DataSourceId = DataSourceId,
            ConnectionId = ConnectionId
        });
    }
}

public class QueryResults
{
    public List<QueryResultColumn> Columns { get; set; } = new List<QueryResultColumn>();
    public List<Dictionary<string, JObject>> Rows { get; set; } = new List<Dictionary<string, JObject>>();

    public static QueryResults FromProtobuf(QueryResponse response)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        var queryResults = new QueryResults();

        // convert rows
        foreach (var row in response.Rows)
        {
            var newRow = new Dictionary<string, JObject>(response.Columns.Count);
            for (int i = 0; i < response.Columns.Count; i++)
            {
                var column = response.Columns[i];
                if (column?.Name != null)
                {
                    newRow[column.Name] = Utils.ConvertStructToJson(row.Inner[i]);
                }
            }
            queryResults.Rows.Add(newRow);
        }

        // convert columns
        foreach (var column in response.Columns)
        {
            if (column == null)
            {
                continue;
            }

            queryResults.Columns.Add(new QueryResultColumn
            {
                Name = column.Name,
                Type = column.Type
            });
        }

        return queryResults;
    }
}

public class QueryResultColumn
{
    public required string Name { get; set; }
    public required string Type { get; set; }
}
