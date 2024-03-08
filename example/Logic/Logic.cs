extern alias Shared;

using Shared.Model;
using Shared.Utils;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

public static class Logic
{
    [RequiresDynamicCode("Allows dynamic code")]
    [RequiresUnreferencedCode("Allows unreferenced code")]
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

        await ResultAgent.SetResult(new Dictionary<string, string> {
            { "status", "success" },
        });
        await LoggingAgent.Info("[END LOGIC]");
    }

    public static async Task HandleError(Context ctx, Exception error)
    {
        if (error is RailwayError railwayError)
        {
            Console.WriteLine($"An error occurred on Logic `{railwayError?.LogicIdentity?.PermanentIdentity}-{railwayError?.LogicIdentity?.Revision}`");
        }

        Console.WriteLine("[ENTER HANDLE ERROR]");
        await LoggingAgent.Log("ERROR", $"Error occurred: {error}");

        await ResultAgent.SetHttpStatusCode(500);
        await ResultAgent.SetResult(new Dictionary<string, string> {
            { "status", "internal error" },
            { "error", $"Error occurred: {error}" },
        });
    }

    private static async Task DatabaseAgentExample()
    {
        var db = await DatabaseAgent.Acquire("database");

        // select SQL statement
        var result = await db.Query("SELECT id, message, timestamp FROM dbo.Log WHERE id < 3;");
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
            Queries = {
                new Query("source_digital_identity", new Query.Match("sourceDigitalIdentity")),
            },
            Size = 10,
        });

        await EventAgent.SearchEventWithPattern(new SearchEventWithPatternRequest
        {
            Sequences = {
                new Sequence {
                    Conditions = {
                        new Condition("source_digital_identity", "sourceDigitalIdentity", Op.Eq),
                        new Condition("label_id", "label_3", Op.Ne),
                    },
                },
                new Sequence {
                    Conditions = {
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
        await fileStorage.CreateDirAll("/file-storage/example");

        // write file
        var content = Encoding.UTF8.GetBytes("hello world");
        await fileStorage.SimplePutFile("/file-storage/example/meow.txt", content);

        // list all files
        var files = await fileStorage.ListFile("/file-storage/example");
        var tasks = new List<Task>();
        foreach (var file in files)
        {
            var task = LoggingAgent.Info($"file name: {file.Name}, file type: {file.Type}");
            tasks.Add(task);
        }
        await Task.WhenAll(tasks);

        // delete file
        await fileStorage.DeleteFile("/file-storage/example/meow.txt");
    }

    [RequiresUnreferencedCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    [RequiresDynamicCode("Calls System.Text.Json.JsonSerializer.Serialize<TValue>(TValue, JsonSerializerOptions)")]
    private static async Task HttpAgentExample()
    {
        var client = await HttpAgent.Acquire("http");

        // send by string
        var httpResponse = await client.Send("/case1");
        httpResponse.EnsureSuccessStatusCode(); // 200
        if (httpResponse.Content != null)
        {
            var responseContent = await httpResponse.Content.ReadAsStringAsync();
            await LoggingAgent.Info(responseContent);
        }

        // send by request with header
        var req = new SendHttpRequest("/case2", new StringContent("some body"));
        req.Headers.Add("X-Loc-Key", "meow");
        await client.Send(req);

        // send by request with JSON body
        var stringPayload = JsonSerializer.Serialize(new Dictionary<string, string> {
            { "key", "value" },
            { "key2", "value2" },
        });
        var stringContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");
        await client.Post(new SendHttpRequest("/case3", stringContent));
    }

    [RequiresDynamicCode("Calls StorageValue.FromJson(Object)")]
    [RequiresUnreferencedCode("Calls StorageValue.FromJson(Object)")]
    private static async Task LocalStorageAgentExample()
    {
        await LocalStorageAgent.Put("doge", StorageValue.FromString("doge"), 300);

        // test string
        var value = await LocalStorageAgent.Get("doge");
        await LoggingAgent.Info($"Set LocalStorage doge={value?.StringValue}");

        // test dictionary
        var dict = new Dictionary<string, string> {
            { "key", "value" },
            { "key2", "value2" },
        };
        await LocalStorageAgent.Put("dict", StorageValue.FromJson(dict));
        var dictBack = await LocalStorageAgent.Get("dict");
        await LoggingAgent.Info($"Set LocalStorage dict={dictBack?.JsonValue}");
    }

    private static async Task SessionStorageAgentExample()
    {
        await SessionStorageAgent.Put("meow", StorageValue.FromString("meow"));

        // test string
        var value = await SessionStorageAgent.Get("meow");
        await LoggingAgent.Info($"Set SessionStorage meow={value?.StringValue}");

        // test custom class
        var person = new Person("Bob");
        await SessionStorageAgent.Put("bob", StorageValue.FromJson(person, PersonSourceGenerationContext.Default.Person));
        var personBack = await SessionStorageAgent.Get("bob");
        await LoggingAgent.Info($"Set SessionStorage bob={personBack?.JsonValue}");
        await LoggingAgent.Info($"Set SessionStorage bob={personBack?.JsonValue?["Name"]}");
    }
}
