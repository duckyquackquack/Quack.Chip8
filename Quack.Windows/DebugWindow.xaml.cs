using System.Windows;

namespace Quack.WPF
{
    public partial class DebugWindow : Window
    {
        public DebugWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            WindowState = WindowState.Maximized;

            InitializeComponent();
        }
    }
}
