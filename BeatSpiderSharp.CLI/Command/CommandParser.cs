using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;

namespace BeatSpiderSharp.CLI.Command;

public class CommandParser
{
    private static readonly Option<string> _inputPreset = new(["--input-preset", "-i"], description: "Input preset file path") { IsRequired = true };
    private static readonly Option<string> _outputPlaylist = new(["--output-playlist", "-o"], description: "Output playlist file path");
    private static readonly Option<string> _outputSongPath = new(["--output-song-path", "-s"], description: "Output song path");
    private static readonly Option<bool> _disablePlaylistOutput = new(["--disable-playlist-output", "-d"], description: "Disable playlist output", getDefaultValue: () => false);
    private static readonly Option<bool> _disableSongDownload = new(["--disable-song-download", "-D"], description: "Disable song download", getDefaultValue: () => false);
    private static readonly Option<bool> _inputIsLegacy = new(["--legacy"], description: "Input preset is in legacy format", getDefaultValue: () => false);
    private static readonly Option<string> _saveConvertedPresetPath = new(["--save-preset"], description: "Save converted preset to file, only works with legacy input preset");
    private static readonly Option<bool> _convertPresetAndExit = new(["--convert-only"], description: "Convert preset and exit", getDefaultValue: () => false);
    private static readonly Option<bool> _verbose = new(["--verbose", "-v"], description: "Verbose filter logging", getDefaultValue: () => false);

    private class OptionsBinder : BinderBase<Options>
    {
        protected override Options GetBoundValue(BindingContext bindingContext)
        {
            return new Options
            {
                InputPreset = bindingContext.ParseResult.GetValueForOption(_inputPreset) ?? "",
                OutputPlaylist = bindingContext.ParseResult.GetValueForOption(_outputPlaylist),
                OutputSongPath = bindingContext.ParseResult.GetValueForOption(_outputSongPath),
                DisablePlaylistOutput = bindingContext.ParseResult.GetValueForOption(_disablePlaylistOutput),
                DisableSongDownload = bindingContext.ParseResult.GetValueForOption(_disableSongDownload),
                InputIsLegacy = bindingContext.ParseResult.GetValueForOption(_inputIsLegacy),
                SaveConvertedPresetPath = bindingContext.ParseResult.GetValueForOption(_saveConvertedPresetPath),
                ConvertPresetAndExit = bindingContext.ParseResult.GetValueForOption(_convertPresetAndExit),
                Verbose = bindingContext.ParseResult.GetValueForOption(_verbose)
            };
        }
    }

    public static ParseResult ParseCommandForHandler(string[] args, Func<Options, Task> rootHandler)
    {
        var rootCommand = new RootCommand("BeatSpider CLI")
        {
            _inputPreset,
            _outputPlaylist,
            _outputSongPath,
            _disablePlaylistOutput,
            _disableSongDownload,
            _inputIsLegacy,
            _saveConvertedPresetPath,
            _convertPresetAndExit,
            _verbose
        };
        rootCommand.TreatUnmatchedTokensAsErrors = true;
        rootCommand.SetHandler(rootHandler, new OptionsBinder());
        
        var parser = new CommandLineBuilder(rootCommand)
            .UseDefaults()
            .EnablePosixBundling(false)
            .Build();
        return parser.Parse(args);
    }
}