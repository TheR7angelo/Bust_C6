using System.Windows.Media;
using Burst_C6.Function;

namespace Burst_C6
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            if (GetVersion.GetUpdate())
            {
                GetVersion.Update();
                GridFooter.Background = Brushes.Crimson;
            }
            else
            {
                GridFooter.Background = Brushes.ForestGreen;
            }

            LabelVersion.Content = Function.AssemblyCl.GetVersionDeploy().ToString();
        }
    }
}