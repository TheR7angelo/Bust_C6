using Ookii.Dialogs.Wpf;

namespace Burst_C6.Function;

public static class GetPath
{
    public static string GetSavePath()
    {
        var dialog = new VistaFolderBrowserDialog { ShowNewFolderButton = true };
        dialog.ShowDialog();

        return dialog.SelectedPath;
    }
}