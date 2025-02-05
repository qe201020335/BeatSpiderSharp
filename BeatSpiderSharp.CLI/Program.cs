using System.CommandLine.Parsing;
using BeatSpiderSharp.CLI.Command;

namespace BeatSpiderSharp.CLI;

public static class Program
{
    private static async Task Main(string[] args)
    {
        await CommandParser.ParseCommandForHandler(args, new BeatSpiderCLI().Run).InvokeAsync();
    }
}