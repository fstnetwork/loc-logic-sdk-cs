using Saffron.Common;

/// <summary>
/// Represents a versioned identity context.
/// </summary>
public class VersionedIdentityContext
{
    public string Name { get; set; }

    public Guid PermanentIdentity { get; set; }

    public int Revision { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedIdentityContext"/> class.
    /// </summary>
    /// <param name="ctx">The gRPC versioned identity context.</param>
    public VersionedIdentityContext(Saffron.Common.VersionedIdentityContext ctx)
    {
        this.Name = ctx.Name;
        this.PermanentIdentity = Utils.ConvertUuidToGuid(ctx.Id.PermanentIdentity);
        this.Revision = ctx.Id.Revision.Value;
    }

    /// <summary>
    /// Extract a new <see cref="VersionedIdentity"/> class from this instance.
    /// </summary>
    public VersionedIdentity ToVersionedIdentity()
    {
        return new VersionedIdentity
        {
            PermanentIdentity = this.PermanentIdentity,
            Revision = this.Revision,
        };
    }
}

/// <summary>
/// Represents a non-versioned identity context.
/// </summary>
public class IdentityContext
{
    public string Name { get; set; }

    public Guid PermanentIdentity { get; set; }

    public IdentityContext(NonVersionedIdentityContext ctx)
    {
        this.Name = ctx.Name;
        this.PermanentIdentity = Utils.ConvertUuidToGuid(ctx.Id);
    }
}

/// <summary>
/// Represents a versioned identity.
/// </summary>
public class VersionedIdentity
{
    public Guid PermanentIdentity { get; set; }

    public int Revision { get; set; }

    public VersionedIdentity() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="VersionedIdentity"/> class.
    /// </summary>
    /// <param name="ctx">The gRPC versioned identity.</param>
    public VersionedIdentity(Saffron.Common.VersionedIdentity identity)
    {
        this.PermanentIdentity = Utils.ConvertUuidToGuid(identity.PermanentIdentity);
        this.Revision = identity.Revision.Value;
    }

    /// <summary>
    /// Convert this instance to a gRPC versioned identity.
    /// </summary>
    /// <param name="ctx">The gRPC versioned identity.</param>
    public Saffron.Common.VersionedIdentity ToProto()
    {
        return new Saffron.Common.VersionedIdentity
        {
            PermanentIdentity = Utils.ConvertGuidToUuid(this.PermanentIdentity),
            Revision = new Saffron.Common.Revision
            {
                Value = this.Revision
            }
        };
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
        this.ExecutionId = new UInt128(taskKey.ExecutionId.HighBits, taskKey.ExecutionId.LowBits);
        this.TaskId = new UInt128(taskKey.TaskId.HighBits, taskKey.TaskId.LowBits);
    }

    public TaskKey(UInt128 ExecutionId, UInt128 TaskId)
    {
        this.ExecutionId = ExecutionId;
        this.TaskId = TaskId;
    }

    public Saffron.Execution.TaskKey ToProto()
    {
        return new Saffron.Execution.TaskKey
        {
            ExecutionId = new U128
            {
                HighBits = (ulong)(this.ExecutionId >> 64),
                LowBits = (ulong)(this.ExecutionId)
            },
            TaskId = new U128
            {
                HighBits = (ulong)(this.TaskId >> 64),
                LowBits = (ulong)(this.TaskId)
            }
        };
    }

    public byte[] ExecutionIdBytes()
    {
        return Utils.UInt128ToBytes(this.ExecutionId);
    }

    public byte[] TaskIdBytes()
    {
        return Utils.UInt128ToBytes(this.TaskId);
    }

    public string ExecutionIdString()
    {
        return Utils.EncodeUint128(this.ExecutionId);
    }

    public string TaskIdString()
    {
        return Utils.EncodeUint128(this.TaskId);
    }
}
