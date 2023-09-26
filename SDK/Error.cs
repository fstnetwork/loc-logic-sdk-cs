using RuntimeServer;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public class ExceptionWrapper
{
    public new IntPtr Pointer;
    public new int Size;

    public ExceptionWrapper() { }

    public ExceptionWrapper(Exception ex)
    {
        var exceptionInfo = new ExceptionInfo
        {
            Type = ex.GetType().ToString(),
            Message = ex.Message,
            StackTrace = ex.StackTrace
        };
        byte[] exceptionBytes = Utils.ConvertToByteArray(exceptionInfo);
        IntPtr exceptionPtr = Marshal.AllocHGlobal(exceptionBytes.Length);
        Marshal.Copy(exceptionBytes, 0, exceptionPtr, exceptionBytes.Length);

        Pointer = exceptionPtr;
        Size = exceptionBytes.Length;
    }

    public ExceptionWrapper(IntPtr pointer)
    {
        var ex = Marshal.PtrToStructure<ExceptionWrapper>(pointer);
        if (ex == null)
        {
            throw new Exception("ExceptionWrapper is null");
        }

        Pointer = ex.Pointer;
        Size = ex.Size;
    }

    public Exception ToLogicError()
    {
        byte[] exceptionBytes = new byte[Size];
        Marshal.Copy(Pointer, exceptionBytes, 0, Size);

        var exceptionInfo = Utils.ToObject<ExceptionInfo>(exceptionBytes);
        return new LogicError(exceptionInfo);
    }

    public void Free()
    {
        Marshal.FreeHGlobal(Pointer);
    }
}

public class LogicError : Exception
{
    public string Type { get; private set; }
    public new string StackTrace { get; private set; }

    public LogicError(ExceptionInfo ex)
        : base(ex.Message)
    {
        this.Type = ex.Type;
        this.StackTrace = ex.StackTrace;
    }

    public override string ToString()
    {
        return $"{Type}, {Message}\n{StackTrace}";
    }
}
