using System.Diagnostics;
using System.IO;
using SuchByte.MacroDeck.GUI;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.InternalPlugins.ActionButtonPlugin.Models;
using SuchByte.MacroDeck.InternalPlugins.ActionButtonPlugin.Views;
using SuchByte.MacroDeck.Language;
using SuchByte.MacroDeck.Logging;
using SuchByte.MacroDeck.Plugins;

namespace SuchByte.MacroDeck.InternalPlugins.ActionButtonPlugin.Actions;

public class LaunchProgramAction : PluginAction
{
    public override string Name => LanguageManager.Strings.ActionLaunchProgram;
    public override string Description => LanguageManager.Strings.ActionLaunchProgramDescription;
    public override bool CanConfigure => true;

    public override void Trigger(string clientId, ActionButton.ActionButton actionButton)
    {
        var config = LaunchProgramActionConfigModel.Deserialize(Configuration);

        if (string.IsNullOrWhiteSpace(config.FilePath) || !File.Exists(config.FilePath))
            return;

        try
        {
            var psi = new ProcessStartInfo(config.FilePath)
            {
                Arguments = config.Arguments ?? "",
                WorkingDirectory = string.IsNullOrWhiteSpace(config.WorkingDirectory)
                    ? Path.GetDirectoryName(config.FilePath) ?? ""
                    : config.WorkingDirectory,
                UseShellExecute = true,
            };
            Process.Start(psi);
        }
        catch { }
    }

    public override void OnActionButtonLoaded()
    {
        try
        {
            var config = LaunchProgramActionConfigModel.Deserialize(Configuration);
            if (!string.IsNullOrWhiteSpace(config.IconPackName) && !string.IsNullOrWhiteSpace(config.IconId))
            {
                var iconRef = config.IconPackName + "." + config.IconId;
                if (ActionButton != null)
                {
                    ActionButton.IconOff = iconRef;
                    ActionButton.IconOn = iconRef;
                    MacroDeckLogger.Info(typeof(LaunchProgramAction), $"Auto-assigned icon {iconRef} to button");
                }
            }
        }
        catch (Exception ex)
        {
            MacroDeckLogger.Warning(typeof(LaunchProgramAction), $"OnActionButtonLoaded: {ex.Message}");
        }
    }

    public override ActionConfigControl GetActionConfigControl(ActionConfigurator actionConfigurator)
    {
        return new LaunchProgramActionConfigView(this);
    }
}
