using System.Runtime.InteropServices;

public static class RunnableLogic
{
    [UnmanagedCallersOnly(EntryPoint = "run")]
    public static IntPtr Run(IntPtr optionPtr)
    {
        initializeFromOption(optionPtr);

        try
        {
            Logic.Run(new Context()).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            // allocate memory for sharing error
            var exWrapper = new ExceptionWrapper(ex);
            IntPtr errorPtr = Marshal.AllocHGlobal(Marshal.SizeOf<ExceptionWrapper>());
            Marshal.StructureToPtr(exWrapper, errorPtr, false);
            return errorPtr;
        }

        return IntPtr.Zero;
    }

    [UnmanagedCallersOnly(EntryPoint = "handleError")]
    public static void HandleError(IntPtr optionPtr, IntPtr errorPtr)
    {
        initializeFromOption(optionPtr);

        // read error from memory pointer
        var exceptionWrapper = new ExceptionWrapper(errorPtr);
        var logicError = exceptionWrapper.ToLogicError();

        Logic.HandleError(new Context(), logicError).GetAwaiter().GetResult();
    }

    private static void initializeFromOption(IntPtr optionPtr)
    {
        // read option from memory pointer
        var optionWrapper = new OptionWrapper(optionPtr);
        var option = optionWrapper.ToRuntimeOption();

        // initialize connected runtime address
        GrpcChannelService.SetGrpcEndpoint(option.RuntimeAddress);

        // initialize current TaskKey
        Global.TaskKey = new TaskKey(option.TaskKey);
    }
}
