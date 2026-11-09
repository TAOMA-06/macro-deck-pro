using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SuchByte.MacroDeck.GUI.CustomControls;
using SuchByte.MacroDeck.InternalPlugins.ActionButtonPlugin.ViewModels;
using SuchByte.MacroDeck.Language;
using SuchByte.MacroDeck.Plugins;

namespace SuchByte.MacroDeck.InternalPlugins.ActionButtonPlugin.Views;

public partial class LaunchProgramActionConfigView : ActionConfigControl
{
    private readonly LaunchProgramActionConfigViewModel _viewModel;

    public LaunchProgramActionConfigView(PluginAction pluginAction)
    {
        InitializeComponent();
        _viewModel = new LaunchProgramActionConfigViewModel(pluginAction);
        lblFilePath.Text = LanguageManager.Strings.FilePath;
        lblArguments.Text = LanguageManager.Strings.Arguments;
        lblWorkingDirectory.Text = LanguageManager.Strings.WorkingDirectory;
        btnBrowse.Text = LanguageManager.Strings.Browse;

        _viewModel.IconExtracted += OnIconExtracted;
    }

    private void LaunchProgramActionConfigView_Load(object sender, EventArgs e)
    {
        txtFilePath.Text = _viewModel.FilePath;
        txtArguments.Text = _viewModel.Arguments;
        txtWorkingDirectory.Text = _viewModel.WorkingDirectory;

        if (_viewModel.HasExtractedIcon && File.Exists(_viewModel.FilePath))
        {
            var pack = Icons.IconManager.GetIconPackByName(_viewModel.IconPackName);
            if (pack != null)
            {
                var icon = Icons.IconManager.GetIcon(pack, _viewModel.IconId);
                if (icon != null)
                {
                    picIconPreview.Image = icon.IconImage;
                    lblExtracted.Text = "✓";
                    lblExtracted.ForeColor = Color.FromArgb(0, 200, 0);
                }
            }
        }
    }

    public override bool OnActionSave()
    {
        _viewModel.FilePath = txtFilePath.Text;
        _viewModel.FileName = Path.GetFileName(txtFilePath.Text);
        _viewModel.Arguments = txtArguments.Text;
        _viewModel.WorkingDirectory = txtWorkingDirectory.Text;
        return _viewModel.SaveConfig();
    }

    private void OnIconExtracted(Image iconImage)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<Image>(OnIconExtracted), iconImage);
            return;
        }
        picIconPreview.Image = iconImage;
        lblExtracted.Text = "✓";
        lblExtracted.ForeColor = Color.FromArgb(0, 200, 0);
    }

    private void BtnBrowse_Click(object sender, EventArgs e)
    {
        using var openFileDialog = new OpenFileDialog
        {
            AddExtension = true,
            DefaultExt = ".exe",
            Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
        };
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            txtFilePath.Text = openFileDialog.FileName;
            _viewModel.ExtractIcon(openFileDialog.FileName);

            if (string.IsNullOrWhiteSpace(txtWorkingDirectory.Text))
            {
                txtWorkingDirectory.Text = Path.GetDirectoryName(openFileDialog.FileName);
            }
        }
    }

    private void BtnWorkingDirBrowse_Click(object sender, EventArgs e)
    {
        using var folderDialog = new FolderBrowserDialog();
        if (!string.IsNullOrWhiteSpace(txtWorkingDirectory.Text))
        {
            folderDialog.SelectedPath = txtWorkingDirectory.Text;
        }
        if (folderDialog.ShowDialog() == DialogResult.OK)
        {
            txtWorkingDirectory.Text = folderDialog.SelectedPath;
        }
    }
}
