extern alias Shared;

using Shared.Utils;
using System.Text;

public static class Logic
{
    public static async Task Run(Context ctx)
    {
        await LoggingAgent.Info("[START LOGIC]");

        var taskKey = (await ctx.GetTask()).TaskKey;
        await LoggingAgent.Debug($"execution_id={taskKey.ExecutionId}, task_id={taskKey.TaskId}");

        var payload = await ctx.GetPayload();
        var httpRequest = payload.Http!.Request;
        await LoggingAgent.Debug($"REQUEST: [{httpRequest.Method}] {httpRequest.Path}?{httpRequest.Query}");

        await CustomLoggingAgent.SimpleLog("Custom logging agent");

        await DatabaseAgentExample();
        await EventAgentExample();
        await FileStorageAgentExample();
        await HttpAgentExample();
        await LocalStorageAgentExample();
        await SessionStorageAgentExample();

        await LoggingAgent.Info("[END LOGIC]");
    }

    public static async Task HandleError(Context ctx, Exception error)
    {
        Console.WriteLine("[ENTER HANDLE ERROR]");
        await LoggingAgent.Log("ERROR", $"Error occurred: {error}");
        await ResultAgent.SetResult(new Dictionary<string, string> {
            { "status", "internal error" },
            { "error", $"Error occurred: {error}" },
        });
    }

    private static async Task DatabaseAgentExample()
    {
        var db = await DatabaseAgent.Acquire("database");

        // select SQL statement
        var result = await db.Query("SELECT id, message, timestamp FROM dbo.Log;");
        foreach (var row in result.Rows)
        {
            await LoggingAgent.Info($@"id: {row["id"]}, message: {row["message"]}, timestamp: {row["timestamp"]}");
        }

        // execute SQL statement
        await db.Execute("SELECT $1, $2, $3;", new List<object> { 1, 2, 3 });

        // transaction
        await db.BeginTransaction();
        await db.Execute("SELECT 1;");
        await db.CommitTransaction();

        // release connection
        await db.Release();
    }

    private static async Task EventAgentExample()
    {
        await EventAgent.EmitEvent(new List<EmitEventArgs>
        {
            new EmitEventArgs("labelName", "sourceDigitalIdentity", "targetDigitalIdentity", "meta", "type"),
        });

        await EventAgent.SearchEvent(new SearchEventRequest
        {
            Queries = new List<Query> {
                new Query("source_digital_identity", new Query.Match("sourceDigitalIdentity")),
            },
            Size = 10,
        });

        await EventAgent.SearchEventWithPattern(new SearchEventWithPatternRequest
        {
            Sequences = new List<Sequence> {
                new Sequence {
                    Conditions = new List<Condition> {
                        new Condition("source_digital_identity", "sourceDigitalIdentity", Op.Eq),
                        new Condition("label_id", "label_3", Op.Ne),
                    },
                },
                new Sequence {
                    Conditions = new List<Condition> {
                        new Condition("label_id", "label_2", Op.Eq),
                    },
                },
            },
            Filter = new Filter(
                "timestamp",
                new Filter.Range
                {
                    Gte = 1625122259000,
                    Lte = 1625124173000,
                }
            ),
        });
    }

    private static async Task FileStorageAgentExample()
    {
        var fileStorage = await FileStorageAgent.Acquire("file-storage");

        // create directory
        await fileStorage.CreateDirAll("file-storage/example");

        // write file
        var content = Encoding.UTF8.GetBytes("hello world");
        await fileStorage.SimplePutFile("file-storage/example/meow.txt", content);

        // list all files
        var files = await fileStorage.ListFile("file-storage/example");
        var tasks = new List<Task>();
        foreach (var file in files)
        {
            var task = LoggingAgent.Info($"file name: {file.Name}, file type: {file.Type}");
            tasks.Add(task);
        }
        await Task.WhenAll(tasks);

        // delete file
        await fileStorage.DeleteFile("file-storage/example/meow.txt");
    }

    private static async Task HttpAgentExample()
    {
        var http = await HttpAgent.Acquire("http");
    }

    private static async Task LocalStorageAgentExample()
    {
        await LocalStorageAgent.Put("doge", StorageValue.FromString("doge"), 300);

        var value = await LocalStorageAgent.Get("doge");
        await LoggingAgent.Info($"Set LocalStorage doge={value.StringValue}");
    }

    private static async Task SessionStorageAgentExample()
    {
        await SessionStorageAgent.Put("meow", StorageValue.FromString("meow"));

        var value = await SessionStorageAgent.Get("meow");
        await LoggingAgent.Info($"Set SessionStorage meow={value.StringValue}");
    }

}
