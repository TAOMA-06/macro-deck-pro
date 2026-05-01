using System.Drawing;
using System.IO;
using SuchByte.MacroDeck.Icons;
using SuchByte.MacroDeck.InternalPlugins.ActionButtonPlugin.Models;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Models;
using SuchByte.MacroDeck.Plugins;
using SuchByte.MacroDeck.ViewModels;

namespace SuchByte.MacroDeck.InternalPlugins.ActionButtonPlugin.ViewModels;

public class LaunchProgramActionConfigViewModel : ISerializableConfigViewModel
{
    private readonly PluginAction _pluginAction;

    public LaunchProgramActionConfigModel Configuration { get; set; }

    ISerializableConfiguration ISerializableConfigViewModel.SerializableConfiguration => Configuration;

    public event Action<Image> IconExtracted;

    public string FilePath
    {
        get => Configuration.FilePath;
        set => Configuration.FilePath = value;
    }

    public string FileName
    {
        get => Configuration.FileName;
        set => Configuration.FileName = value;
    }

    public string Arguments
    {
        get => Configuration.Arguments;
        set => Configuration.Arguments = value;
    }

    public string WorkingDirectory
    {
        get => Configuration.WorkingDirectory;
        set => Configuration.WorkingDirectory = value;
    }

    public string IconPackName
    {
        get => Configuration.IconPackName;
        set => Configuration.IconPackName = value;
    }

    public string IconId
    {
        get => Configuration.IconId;
        set => Configuration.IconId = value;
    }

    public bool HasExtractedIcon => !string.IsNullOrWhiteSpace(Configuration.IconPackName)
                                    && !string.IsNullOrWhiteSpace(Configuration.IconId);

    public LaunchProgramActionConfigViewModel(PluginAction pluginAction)
    {
        _pluginAction = pluginAction;
        Configuration = LaunchProgramActionConfigModel.Deserialize(pluginAction.Configuration);
    }

    public void ExtractIcon(string exePath)
    {
        try
        {
            using var icon = System.Drawing.Icon.ExtractAssociatedIcon(exePath);
            if (icon == null) return;

            using var bitmap = icon.ToBitmap();
            var iconId = Path.GetFileNameWithoutExtension(exePath);

            var pack = IconManager.GetIconPackByName("Application Icons");
            if (pack == null)
            {
                IconManager.CreateIconPack("Application Icons", "Macro Deck", "1.0");
                pack = IconManager.GetIconPackByName("Application Icons");
            }

            if (pack == null) return;

            var existingIcon = IconManager.GetIcon(pack, iconId);
            if (existingIcon != null)
            {
                IconManager.DeleteIcon(pack, existingIcon);
            }

            var addedIcon = IconManager.AddIconImage(pack, bitmap, iconId);
            if (addedIcon != null)
            {
                Configuration.IconPackName = pack.Name;
                Configuration.IconId = iconId;
                MacroDeckLogger.Info(typeof(LaunchProgramActionConfigViewModel), $"Icon extracted: {pack.Name}.{iconId}");
                IconExtracted?.Invoke(addedIcon.IconImage);
            }
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Warning(typeof(LaunchProgramActionConfigViewModel), $"Failed to extract icon: {ex.Message}");
        }
    }

    public bool SaveConfig()
    {
        try
        {
            SetConfig();
            MacroDeckLogger.Info(typeof(LaunchProgramActionConfigViewModel), "config saved");
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Error(typeof(LaunchProgramActionConfigViewModel), $"Error while saving config: {ex.Message + Environment.NewLine + ex.StackTrace}");
        }
        return true;
    }

    public void SetConfig()
    {
        _pluginAction.ConfigurationSummary = string.IsNullOrWhiteSpace(Configuration.FileName)
            ? Language.LanguageManager.Strings.ActionLaunchProgram
            : Configuration.FileName;
        _pluginAction.Configuration = Configuration.Serialize();
    }
}
