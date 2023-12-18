using Google.Protobuf;
using Saffron.Common;

public class VersionedIdentityContext
{
    public string Name { get; set; }

    public Guid PermanentIdentity { get; set; }

    public int Revision { get; set; }

    public VersionedIdentityContext(Saffron.Common.VersionedIdentityContext ctx)
    {
        this.Name = ctx.Name;
        this.PermanentIdentity = new Guid(ctx.Id.PermanentIdentity.ToByteArray());
        this.Revision = ctx.Id.Revision.Value;
    }
}

public class IdentityContext
{
    public string Name { get; set; }

    public Guid PermanentIdentity { get; set; }

    public IdentityContext(NonVersionedIdentityContext ctx)
    {
        this.Name = ctx.Name;
        this.PermanentIdentity = new Guid(ctx.Id.ToByteArray());
    }
}

public class LogicTask
{
    public TaskKey TaskKey { get; set; }
    public DateTime StartTimestamp { get; set; }
    public VersionedIdentityContext DataProcess { get; set; }
    public VersionedIdentityContext CurrentLogic { get; set; }
    public List<VersionedIdentityContext> ExecutedLogics { get; set; }

    public LogicTask(Saffron.Execution.Task task)
    {
        this.TaskKey = new TaskKey(task.TaskKey);
        this.StartTimestamp = task.StartTimestamp.ToDateTime();
        this.DataProcess = new VersionedIdentityContext(task.DataProcess);
        this.CurrentLogic = new VersionedIdentityContext(task.CurrentLogic);

        this.ExecutedLogics = new List<VersionedIdentityContext>();
        foreach (var logic in task.ExecutedLogics)
        {
            this.ExecutedLogics.Add(new VersionedIdentityContext(logic));
        }
    }
}

public class TaskKey
{
    public UInt128 ExecutionId { get; set; }
    public UInt128 TaskId { get; set; }

    public TaskKey(Saffron.Execution.TaskKey taskKey)
    {
        this.ExecutionId = BytesToUInt128(taskKey.ExecutionId.Value.ToByteArray());
        this.TaskId = BytesToUInt128(taskKey.TaskId.Value.ToByteArray());
    }

    public TaskKey(UInt128 ExecutionId, UInt128 TaskId)
    {
        this.ExecutionId = ExecutionId;
        this.TaskId = TaskId;
    }

    private static UInt128 BytesToUInt128(byte[] bytes)
    {
        if (bytes.Length != 16)
        {
            throw new ArgumentException("Byte array must be exactly 16 bytes long.");
        }

        ulong upper = BitConverter.ToUInt64(bytes, 0);
        ulong lower = BitConverter.ToUInt64(bytes, 8);

        return new UInt128(upper, lower);
    }

    public byte[] ExecutionIdBytes()
    {
        var executionId = this.ExecutionId;
        byte[] bytes = new byte[16];
        for (int i = 15; i >= 0; i--)
        {
            bytes[i] = (byte)(executionId % 256);
            executionId /= 256;
        }
        return bytes;
    }

    public byte[] TaskIdBytes()
    {
        var taskId = this.TaskId;
        byte[] bytes = new byte[16];
        for (int i = 15; i >= 0; i--)
        {
            bytes[i] = (byte)(taskId % 256);
            taskId /= 256;
        }
        return bytes;
    }

    public string executionIdString()
    {
        var bytes = ExecutionIdBytes();
        string base64 = Convert.ToBase64String(bytes);
        return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    public string taskIdString()
    {
        var bytes = TaskIdBytes();
        string base64 = Convert.ToBase64String(bytes);
        return base64.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    public Saffron.Execution.TaskKey ToProto()
    {
        return new Saffron.Execution.TaskKey
        {
            ExecutionId = new U128 { Value = ByteString.CopyFrom(ExecutionIdBytes()) },
            TaskId = new U128 { Value = ByteString.CopyFrom(TaskIdBytes()) }
        };
    }
}
