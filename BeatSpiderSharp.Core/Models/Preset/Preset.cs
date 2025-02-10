namespace BeatSpiderSharp.Core.Models.Preset;

public class Preset
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string Author { get; set; } = string.Empty;

    public InputConfig Input { get; set; } = new InputConfig();

    public OutputConfig Output { get; set; } = new OutputConfig();

    /// <summary>
    /// Multiple instances applied as OR
    /// </summary>
    public IList<FilterConfig> FilterOptions { get; set; } = new List<FilterConfig>(1);
}