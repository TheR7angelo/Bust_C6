using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Ookii.Dialogs.Wpf;
using Syroot.Windows.IO;
using UpdateManager;

namespace Bust_C6.Function;

public static class GetVersion
{
    private static readonly bool SignleFile = true;
    private static string NewVersion { get; set; } = string.Empty;
    private static string NewVersionCopy { get; set; } = string.Empty;

    private static readonly Manager Manager = new Manager("T:\\- 4 Suivi Appuis\\18-Partage\\BARRENTO ANTUNES Raphael\\7_CSharp\\Centre macro.db");

    public static bool GetUpdate()
    {
        var result = false;
        try
        {
            var last = Manager.GetLastVersion(AssemblyCl.GetNameDeploy());

            if (last is not null)
            {
                var dbVersion = last.Value.Version;
                NewVersion = last.Value.Link;
                result = dbVersion > AssemblyCl.GetVersionDeploy();
            }
        }
        catch (Exception) { /* ignored */ }

        return result;
    }

    public static void Update()
    {
        var buttonYes = new TaskDialogButton
        {
            ButtonType = ButtonType.Yes,
            Default = true
        };
        
        var buttonNo = new TaskDialogButton
        {
            ButtonType = ButtonType.No,
            Default = true,
        };
        
        var msg = new TaskDialog
        {
            WindowTitle = "Une mise à jour est disponible",
            MainIcon = TaskDialogIcon.Information,
            Content = "Une mise à jour est disponible voulez l'installer ?",
            AllowDialogCancellation = false,
            Buttons = { buttonYes, buttonNo }
        };

        var result = msg.ShowDialog();
        Msg_OnButtonClicked(result);
    }

    private static void Msg_OnButtonClicked(TaskDialogButton sender)
    {
        if (!sender.ButtonType.Equals(ButtonType.Yes)) return;
        
        var msg = new ProgressDialog
        {
            WindowTitle = "Mise à jour",
            Text = "Téléchargement en cours",
            ShowCancelButton = true,
            ShowTimeRemaining = true,
        };
        msg.DoWork += Msg_OnDoWork;
        msg.RunWorkerCompleted += Msg_OnRunWorkerCompleted;
            
        msg.Show();
    }

    private static void Msg_OnRunWorkerCompleted(object? sender, RunWorkerCompletedEventArgs e)
    {
        if (!File.Exists(NewVersionCopy)) return;
        
        if (SignleFile)
        {
            var currentDirectory = AssemblyCl.GetFolderPath();
            currentDirectory = Path.GetDirectoryName(currentDirectory);
            Console.WriteLine(currentDirectory);
            File.Move(NewVersionCopy, Path.Combine(currentDirectory!, Path.GetFileName(NewVersionCopy)), true);
        }
        else
        {
            Process.Start(NewVersionCopy);
        }
    }

    private static void Msg_OnDoWork(object? sender, DoWorkEventArgs e)
    {
        var msg = (ProgressDialog)sender!;

        var progress = 0.0;
        
        NewVersionCopy = GetOutput();

        var isComplete = false;
        var copy = new CustomFileCopy(NewVersion, NewVersionCopy);
        copy.OnProgressChanged += delegate(double percentage) { progress = percentage; };
        copy.OnComplete += delegate { isComplete = true; };

        Task.Run(() =>
        {
            copy.Start();
        });
        
        while (!isComplete)
        {
            if (msg.CancellationPending)
            {
                copy.Cancel = true;
                break;
            }
            
            Console.WriteLine(progress);
            msg.ReportProgress((int)progress, null, $"Progression: {Math.Round(progress, 2)} %");
        }
    }

    private static string GetOutput()
    {
        var output = KnownFolders.Downloads.Path;
        return Path.Combine(output, Path.GetFileName(NewVersion));
    }
}