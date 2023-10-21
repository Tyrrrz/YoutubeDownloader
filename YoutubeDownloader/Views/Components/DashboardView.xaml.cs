using System.Windows.Input;

namespace YoutubeDownloader.Views.Components;

public partial class DashboardView
{
    public DashboardView()
    {
        InitializeComponent();
    }

    private void QueryTextBox_OnPreviewKeyDown(object sender, KeyEventArgs args)
    {
        // Disable new lines when pressing enter without shift
        if (args.Key == Key.Enter && args.KeyboardDevice.Modifiers != ModifierKeys.Shift)
        {
            args.Handled = true;

            // We handle the event here so we have to directly "press" the default button
            AccessKeyManager.ProcessKey(null, "\x000D", false);
        }
    }
}
