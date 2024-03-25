using System.Runtime.InteropServices;

public static class DummyLogic
{

    [UnmanagedCallersOnly(EntryPoint = "run")]
    public static IntPtr Run(IntPtr optionPtr)
    {
        return IntPtr.Zero;
    }

    [UnmanagedCallersOnly(EntryPoint = "handleError")]
    public static void HandleError(IntPtr optionPtr, IntPtr errorPtr)
    {
    }
}
