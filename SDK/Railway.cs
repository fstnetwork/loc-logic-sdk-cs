using Saffron.ExternalRuntime;

public class RailwayError : Exception
{
    public string Name { get; }
    public override string Message { get; }
    public override string StackTrace { get; } = "";
    public VersionedIdentity? LogicIdentity { get; }

    public RailwayError(Saffron.Runtime.RailwayError error) : base(error.Message)
    {
        Name = error.Name;
        Message = error.Message;
        LogicIdentity = new VersionedIdentity(error.LogicIdentity);
    }

    public RailwayError(RailwayErrorInner error)
        : base(error.Message)
    {
        Name = error.Name;
        Message = error.Message;
        StackTrace = error.StackTrace;
        LogicIdentity = new VersionedIdentity(error.LogicIdentity);
    }

    public override string ToString()
    {        
        return $"{Name}, {Message}\n{StackTrace}";
    }
}
