using System.Windows;

namespace Bust_C6.Views;

public partial class MainView
{
    public MainView()
    {
        InitializeComponent();
    }
    
    private void Grid_Drop(object sender, DragEventArgs e) {
        if (null != e.Data && e.Data.GetDataPresent(DataFormats.FileDrop)) {
            var data = e.Data.GetData(DataFormats.FileDrop) as string[];
            // handle the files here!
        }
    }
 
    private void Grid_DragOver(object sender, DragEventArgs e) {
        if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
            e.Effects = DragDropEffects.Copy;
        } else {
            e.Effects = DragDropEffects.None;
        }
    }
}