using Saffron.ExternalRuntime;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
public class RailwayErrorWrapper
{
    public IntPtr Pointer;
    public int Size;

    public RailwayErrorWrapper() { }

    public RailwayErrorWrapper(Exception ex)
    {
        var railwayErrorInner = new RailwayErrorInner
        {
            Name = ex.GetType().ToString(),
            Message = ex.Message,
            StackTrace = ex.StackTrace
        };
        byte[] errorBytes = Utils.ConvertToByteArray(railwayErrorInner);
        IntPtr errorPtr = Marshal.AllocHGlobal(errorBytes.Length);
        Marshal.Copy(errorBytes, 0, errorPtr, errorBytes.Length);

        Pointer = errorPtr;
        Size = errorBytes.Length;
    }

    public RailwayErrorWrapper(Exception ex, VersionedIdentity currentLogicIdentity)
    {
        var railwayErrorInner = new RailwayErrorInner
        {
            Name = ex.GetType().ToString(),
            Message = ex.Message,
            StackTrace = ex.StackTrace,
            LogicIdentity = currentLogicIdentity.ToProto(),
        };
        byte[] errorBytes = Utils.ConvertToByteArray(railwayErrorInner);
        IntPtr errorPtr = Marshal.AllocHGlobal(errorBytes.Length);
        Marshal.Copy(errorBytes, 0, errorPtr, errorBytes.Length);

        Pointer = errorPtr;
        Size = errorBytes.Length;
    }

    public RailwayErrorWrapper(IntPtr pointer)
    {
        var ex = Marshal.PtrToStructure<RailwayErrorWrapper>(pointer);
        if (ex == null)
        {
            throw new Exception("ExceptionWrapper is null");
        }

        Pointer = ex.Pointer;
        Size = ex.Size;
    }

    public RailwayErrorWrapper(RailwayError? railwayError)
    {
        var railwayErrorInner = new RailwayErrorInner
        {
            Name = railwayError?.Name ?? "CustomError",
            Message = railwayError?.Message ?? "Missing railway error",
            LogicIdentity = railwayError?.LogicIdentity?.ToProto(),
        };
        byte[] exceptionBytes = Utils.ConvertToByteArray(railwayErrorInner);
        IntPtr exceptionPtr = Marshal.AllocHGlobal(exceptionBytes.Length);
        Marshal.Copy(exceptionBytes, 0, exceptionPtr, exceptionBytes.Length);

        Pointer = exceptionPtr;
        Size = exceptionBytes.Length;
    }

    public Exception ToRailwayError()
    {
        byte[] exceptionBytes = new byte[Size];
        Marshal.Copy(Pointer, exceptionBytes, 0, Size);

        var railwayErrorInner = Utils.ToObject<RailwayErrorInner>(exceptionBytes);
        if (railwayErrorInner == null)
        {
            throw new Exception("RailwayErrorInner is null");
        }

        return new RailwayError(railwayErrorInner);
    }

    public void Free()
    {
        Marshal.FreeHGlobal(Pointer);
    }
}
