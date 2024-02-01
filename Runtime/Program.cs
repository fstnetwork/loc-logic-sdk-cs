using CommandLine;
using System.Runtime.InteropServices;
using Saffron.ExternalRuntime;

class Runtime
{
    /* Declare a function in a shared library */
    [DllImport("lib_loc_logic", EntryPoint = "run")]
    private static extern IntPtr run(IntPtr optionPtr);
    [DllImport("lib_loc_logic", EntryPoint = "handleError")]
    private static extern void handleError(IntPtr optionPtr, IntPtr errorPtr);

    class Options
    {
        [Option("runtime-address", Required = false, HelpText = "The gRPC address of Runtime server")]
        public required string? RuntimeAddress { get; set; }

        [Option("execution-id", Required = true, HelpText = "The Execution ID in UInt128 format")]
        public required UInt128 ExecutionId { get; set; }

        [Option("task-id", Required = true, HelpText = "The Task ID in UInt128 format")]
        public required UInt128 TaskId { get; set; }
    }

    static async Task<int> Main(string[] args)
    {
        var options = ParseArguments(args);
        if (options == null)
        {
            return 1;
        }

        Console.WriteLine($".NET Runtime starting...");
        Console.WriteLine($"Execute Task with execution_id={Utils.EncodeUint128(options.ExecutionId)} task_id={Utils.EncodeUint128(options.TaskId)}");

        return await Execute(options);
    }

    static Options? ParseArguments(string[] args)
    {
        Options? options = null;

        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(opts => options = opts)
            .WithNotParsed(errs =>
            {
                var unhandledErrors = errs.Where(err => !(err is HelpRequestedError || err is VersionRequestedError)).ToList();
                if (unhandledErrors.Any())
                {
                    Console.WriteLine("Argument parsing error occurred:");
                    foreach (var err in unhandledErrors)
                    {
                        Console.WriteLine($"Error: {err}");
                    }
                }
            });

        if (options != null)
        {
            options.RuntimeAddress = options.RuntimeAddress ?? Environment.GetEnvironmentVariable(Constants.GrpcEndpointEnvVar);
            if (string.IsNullOrEmpty(options.RuntimeAddress))
            {
                Console.WriteLine("Missing agreement `runtime-address`.");
                return null;
            }
            GrpcChannelService.SetGrpcEndpoint(options.RuntimeAddress);
        }

        return options;
    }

    static async Task<int> Execute(Options options)
    {
        // initialize current TaskKey
        Global.TaskKey = new TaskKey(options.ExecutionId, options.TaskId);

        // prepare runtime option
        var runtimeOption = new RuntimeOption
        {
            TaskKey = Global.TaskKey.ToProto(),
            RuntimeAddress = options.RuntimeAddress,
        };
        var optionWrapper = new RuntimeOptionWrapper(runtimeOption);
        IntPtr optionPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(RuntimeOptionWrapper)));
        Marshal.StructureToPtr(optionWrapper, optionPtr, false);

        // execute Logic Railways
        if (await LogicRailway.IsRailwayOk())
        {
            var errorPtr = run(optionPtr);
            if (errorPtr == IntPtr.Zero)
            {
                return 0;
            }

            // read error from memory pointer, and switch railway
            var railwayErrorWrapper = new RailwayErrorWrapper(errorPtr);
            var railwayError = railwayErrorWrapper.ToRailwayError();

            await LogicRailway.SwitchRailway(railwayError.GetType().ToString(), railwayError.Message);

            handleError(optionPtr, errorPtr);
            return 0;
        }
        else
        {
            // allocate memory for sharing error
            var railwayError = await LogicRailway.GetRailwayError();
            var exWrapper = new RailwayErrorWrapper(railwayError);
            IntPtr errorPtr = Marshal.AllocHGlobal(Marshal.SizeOf<RailwayErrorWrapper>());
            Marshal.StructureToPtr(exWrapper, errorPtr, false);

            handleError(optionPtr, errorPtr);
            return 0;
        }
    }
}
