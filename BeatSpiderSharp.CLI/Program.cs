using System.Text.Encodings.Web;
using System.Text.Json;
using BeatSpiderSharp.Core.Models;

namespace BeatSpiderSharp.CLI;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        
        var beatSpider = new BeatSpiderCLI();

        await beatSpider.Run(args);
    }
}