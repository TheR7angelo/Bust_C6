﻿using System.Windows;
using System.Windows.Controls;
using Ookii.Dialogs.Wpf;

namespace Bust_C6.Views;

public partial class MainView
{
    public MainView()
    {
        InitializeComponent();
    }

    #region Function

    private void GetFile(object sender)
    {
        var button = sender as Button;
        var title = button!.Uid.Equals("C3A") ? TextBoxC3A.Text : TextBoxC6.Text;
        
        var dialog = new VistaOpenFileDialog
        {
            Title = title,
            Filter = "Fichier Excel (*.xlsx)|*.xlsx",
            Multiselect = false
        };
        if (dialog.ShowDialog() ?? true)
        {
            SetText(sender, dialog.FileNames[0]);
        }
        
    }
    
    private void SetText(object sender, string value)
    {
        var senderObj = sender as UIElement;
        var dst = senderObj!.Uid;
        switch (dst)
        {
            case "C3A":
                TextBoxC3A.Text = value;
                break;
            case "C6":
                TextBoxC6.Text = value;
                break;
        }
    }

    #endregion

    #region Actions

    private void ButtonGetFile_OnClick(object sender, RoutedEventArgs e) => GetFile(sender);
    
    private void UIElement_OnDrop(object sender, DragEventArgs e)
    {
        
        if (null == e.Data || !e.Data.GetDataPresent(DataFormats.FileDrop)) return;
        var files = e.Data.GetData(DataFormats.FileDrop) as string[];
        if (files is { Length: 0 } or null) return;

        SetText(sender, files[0]);
    }

    private void UIElement_OnPreviewDragOver(object sender, DragEventArgs e) => e.Handled = true;

    #endregion
}