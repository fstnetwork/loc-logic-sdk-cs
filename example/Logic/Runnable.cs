using System.Runtime.InteropServices;

public static class RunnableGenericLogic
{
    public class Options
    {
        public required string? RuntimeAddress { get; set; }
    }

    [UnmanagedCallersOnly(EntryPoint = "run")]
    public static IntPtr Run(IntPtr contextPtr)
    {
        var contextString = Marshal.PtrToStringAnsi(contextPtr) ?? "";
        var ctx = new GenericContext(contextString);

        try
        {
            GenericLogic.run(ctx).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            var exWrapper = new ExceptionWrapper(ex);
            IntPtr exPtr = Marshal.AllocHGlobal(Marshal.SizeOf<ExceptionWrapper>());
            Marshal.StructureToPtr(exWrapper, exPtr, false);
            return exPtr;
        }

        return IntPtr.Zero;
    }

    [UnmanagedCallersOnly(EntryPoint = "handleError")]
    public static void HandleError(IntPtr contextPtr, IntPtr errorPtr)
    {
        // read context
        var contextString = Marshal.PtrToStringAnsi(contextPtr) ?? "";
        var ctx = new GenericContext(contextString);

        // read error
        var exceptionWrapper = new ExceptionWrapper(errorPtr);
        var logicError = exceptionWrapper.ToLogicError();

        GenericLogic.handleError(ctx, logicError).GetAwaiter().GetResult();
    }

    [UnmanagedCallersOnly(EntryPoint = "version")]
    public static IntPtr Version()
    {
        string versionString = "0.0.1";
        return Marshal.StringToHGlobalAnsi(versionString);
    }
}
