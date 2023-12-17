using CommandLine;
using System.Runtime.InteropServices;

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

        [Option("is-ok", Default = true, HelpText = "Choose the railway type is `Run` or `HandleError`")]
        public required bool IsOk { get; set; }

        [Option("ctx", Required = true, HelpText = "Context of the Logic in Base64 encoded string")]
        public required string Context { get; set; }

        [Option("error", Required = false, HelpText = "Error of the Logic in Base64 encoded string. It is only used when `railway-type` is `HandleError`")]
        public required string Error { get; set; }
    }

    static int Main(string[] args)
    {
        var options = ParseArguments(args);
        if (options == null)
        {
            return 1;
        }

        return Execute(options);
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

            if (options.IsOk == false && string.IsNullOrEmpty(options.Error))
            {
                Console.WriteLine("Missing agreement `error`.\nWhen railway-type is `HandleError`, `error` is required.");
                return null;
            }
        }

        return options;
    }

    static int Execute(Options options)
    {
        var ctx = Marshal.StringToHGlobalAnsi(options.RuntimeAddress);

        if (options.IsOk)
        {
            var exPtr = run(ctx);
            if (exPtr == IntPtr.Zero)
            {
                return 0;
            }

            handleError(ctx, exPtr);
            return 0;
        }
        else
        {
            handleError(ctx, Marshal.StringToHGlobalAnsi(options.Error));
            return 0;
        }
    }
}
