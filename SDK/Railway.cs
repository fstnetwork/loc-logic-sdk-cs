using Saffron.Common;

public class RailwayError : Exception
{
    public string Name { get; }
    public override string Message { get; }
    public VersionedIdentity LogicIdentity { get; }

    public RailwayError(Saffron.Runtime.RailwayError error) : base(error.Message)
    {
        Name = error.Name;
        Message = error.Message;
        LogicIdentity = new VersionedIdentity(error.LogicIdentity);
    }
}
