using Google.Protobuf.WellKnownTypes;
using Saffron.Runtime;

public static class DatabaseAgent
{
    public async static Task<DatabaseClient> Acquire(string name)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        var resp = await client.AcquireDatabaseAsync(new AcquireDatabaseRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            Name = name,
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

        await client.ReleaseDatabaseAsync(new ReleaseDatabaseRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            DataSourceId = DataSourceId,
            ConnectionId = ConnectionId,
        });
    }

    public async Task<QueryResults> Query(string rawSql, IEnumerable<object>? parameters = null)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        parameters ??= new List<object>();
        var parameterList = new ListValue();
        foreach (var param in parameters)
        {
            parameterList.Values.Add(Utils.ConvertObjectToValue(param));
        }

        var response = await client.QueryDatabaseAsync(new QueryDatabaseRequest
        {
            TaskKey = Global.TaskKey.ToProto(),
            DataSourceId = DataSourceId,
            ConnectionId = ConnectionId,
            RawSql = rawSql,
            Params = parameterList,
        });

        return QueryResults.FromProtobuf(response);
    }

    public async Task<QueryResults> Execute(string rawSql, IEnumerable<object>? parameters = null)
    {
        var channel = GrpcChannelService.GetChannel();
        var client = new Runtime.RuntimeClient(channel);

        parameters ??= new List<object>();
        var parameterList = new ListValue();
        foreach (var param in parameters)
        {
            parameterList.Values.Add(Utils.ConvertObjectToValue(param));
        }

        var response = await client.QueryDatabaseAsync(new QueryDatabaseRequest
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
            ConnectionId = ConnectionId,
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
            ConnectionId = ConnectionId,
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
            ConnectionId = ConnectionId,
        });
    }
}

public class QueryResults
{
    public List<QueryResultColumn> Columns { get; set; } = new List<QueryResultColumn>();
    public List<Dictionary<string, object?>> Rows { get; set; } = new List<Dictionary<string, object?>>();

    public static QueryResults FromProtobuf(QueryDatabaseResponse response)
    {
        if (response == null)
        {
            throw new ArgumentNullException(nameof(response));
        }

        var queryResults = new QueryResults();

        // convert rows
        foreach (var row in response.Rows)
        {
            var newRow = new Dictionary<string, object?>(response.Columns.Count);
            for (int i = 0; i < response.Columns.Count; i++)
            {
                var columnName = response.Columns[i]?.Name;
                if (columnName != null)
                {
                    newRow[columnName] = Utils.ConvertValueToJson(row.Values[i]);
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
                Type = column.Type,
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
