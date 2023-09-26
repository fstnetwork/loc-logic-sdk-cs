using CommandLine;
using System.Runtime.InteropServices;
using RuntimeServer;
using Google.Protobuf;

class Runtime
{
    /* Declare a function in a shared library */
    [DllImport("lib_loc_logic", EntryPoint = "run")]
    private static extern IntPtr run(IntPtr contextPtr);
    [DllImport("lib_loc_logic", EntryPoint = "handleError")]
    private static extern void handleError(IntPtr contextPtr, IntPtr errorPtr);

    class Options
    {
        [Option("runtime-address", Required = false, HelpText = "The gRPC address of Runtime server")]
        public required string? RuntimeAddress { get; set; }
    }

    static int Main(string[] args)
    {
        var options = ParseArguments(args);
        if (options == null)
        {
            return 1;
        }

        Execute(options);
        return 0;
    }

    static Options? ParseArguments(string[] args)
    {
        Options? options = null;

        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(opts => options = opts)
            .WithNotParsed(errs =>
            {
                Console.WriteLine("Argument parsing error occurred:");
                foreach (var err in errs)
                {
                    Console.WriteLine(err.ToString());
                }
            });

        if (options != null)
        {
            options.RuntimeAddress = options.RuntimeAddress ?? Environment.GetEnvironmentVariable(Constants.GrpcEndpointEnvVar);
            if (string.IsNullOrEmpty(options.RuntimeAddress))
            {
                Console.WriteLine("Missing `runtime-address`.");
                return null;
            }
        }

        return options;
    }

    static void Execute(Options options)
    {
        var ctx = Marshal.StringToHGlobalAnsi(options.RuntimeAddress);

        var exPtr = run(ctx);
        if (exPtr == IntPtr.Zero)
        {
            return;
        }

        handleError(ctx, exPtr);

        // var exWrapper = new ExceptionWrapper(ex);
        // IntPtr exPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(ExceptionWrapper)));

        // try
        // {
        //     Marshal.StructureToPtr(exWrapper, exPtr, false);
        //     handleError(ctx, exPtr);
        // }
        // finally
        // {
        //     exWrapper.Free();
        //     Marshal.FreeHGlobal(exPtr);
        // }
    }
}
