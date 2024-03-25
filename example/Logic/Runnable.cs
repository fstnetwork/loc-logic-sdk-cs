using System.Runtime.InteropServices;

public static class RunnableLogic
{
    [UnmanagedCallersOnly(EntryPoint = "run")]
    public static IntPtr RunAot(IntPtr optionPtr)
    {
        return Run(optionPtr);
    }

    public static IntPtr Run(IntPtr optionPtr)
    {
        initializeFromOption(optionPtr);

        var context = new Context();

        try
        {
            Logic.Run(context).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            RailwayErrorWrapper errorWrapper;
            var currentLogic = context.GetTask().GetAwaiter().GetResult()?.CurrentLogic ?? null;
            if (currentLogic == null)
            {
                errorWrapper = new RailwayErrorWrapper(ex);
            }
            else
            {
                errorWrapper = new RailwayErrorWrapper(ex, currentLogic.ToVersionedIdentity());
            }

            // allocate memory for sharing error
            IntPtr errorPtr = Marshal.AllocHGlobal(Marshal.SizeOf<RailwayErrorWrapper>());
            Marshal.StructureToPtr(errorWrapper, errorPtr, false);
            return errorPtr;
        }

        return IntPtr.Zero;
    }

    [UnmanagedCallersOnly(EntryPoint = "handleError")]
    public static void HandleErrorAot(IntPtr optionPtr, IntPtr errorPtr)
    {
        HandleError(optionPtr, errorPtr);
    }

    public static void HandleError(IntPtr optionPtr, IntPtr errorPtr)
    {
        initializeFromOption(optionPtr);

        // read error from memory pointer
        var railwayErrorWrapper = new RailwayErrorWrapper(errorPtr);
        var railwayError = railwayErrorWrapper.ToRailwayError();

        Logic.HandleError(new Context(), railwayError).GetAwaiter().GetResult();
    }

    private static void initializeFromOption(IntPtr optionPtr)
    {
        // read option from memory pointer
        var optionWrapper = new RuntimeOptionWrapper(optionPtr);
        var option = optionWrapper.ToRuntimeOption();

        // initialize connected runtime address
        GrpcChannelService.SetGrpcEndpoint(option.RuntimeAddress);

        // initialize current TaskKey
        Global.TaskKey = new TaskKey(option.TaskKey);
    }
}
