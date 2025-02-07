using System.CommandLine.Parsing;
using BeatSpiderSharp.CLI.Command;

namespace BeatSpiderSharp.CLI;

public static class Program
{
    private static async Task Main(string[] args)
    {
        var code = 0;
        var parsed = CommandParser.ParseCommandForHandler(args, async options =>
        {
            try
            {
                code = await new BeatSpiderCLI().Run(options);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unhandled exception:");
                Console.WriteLine(e);
                code = -1;
            }
        });
        var result = await parsed.InvokeAsync();
        if (result != 0)
        {
            code = result;
        }

        Environment.ExitCode = code;
    }
}