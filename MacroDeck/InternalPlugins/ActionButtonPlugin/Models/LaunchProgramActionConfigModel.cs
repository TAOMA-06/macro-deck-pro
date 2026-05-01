using System.Text.Json;
using SuchByte.MacroDeck.Models;

namespace SuchByte.MacroDeck.InternalPlugins.ActionButtonPlugin.Models;

public class LaunchProgramActionConfigModel : ISerializableConfiguration
{
    public string FilePath { get; set; } = "";

    public string FileName { get; set; } = "";

    public string Arguments { get; set; } = "";

    public string WorkingDirectory { get; set; } = "";

    public string IconPackName { get; set; } = "";

    public string IconId { get; set; } = "";

    public string Serialize()
    {
        return JsonSerializer.Serialize(this);
    }

    public static LaunchProgramActionConfigModel Deserialize(string config)
    {
        return ISerializableConfiguration.Deserialize<LaunchProgramActionConfigModel>(config);
    }
}
