public class VersionedIdentityContext
{
    public string Name { get; set; }

    public string PermanentIdentity { get; set; }

    public int Revision { get; set; }
}

public class IdentityContext
{
    public string Name { get; set; }

    public string PermanentIdentity { get; set; }
}

public class LogicTask
{
    public LogicTaskKey TaskKey { get; set; }
    public DateTime StartTimestamp { get; set; }
    public VersionedIdentityContext DataProcess { get; set; }
    public VersionedIdentityContext CurrentLogic { get; set; }
    public List<VersionedIdentityContext> ExecutedLogics { get; set; }

    public LogicTask()
    {
        ExecutedLogics = new List<VersionedIdentityContext>();
    }
}

public class LogicTaskKey
{
    public string ExecutionId { get; set; }
    public string TaskId { get; set; }
}
