using IDT2025.Properties;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace IDT2025
{
    public partial class Dashboard : UserControl
    {
        public string FirstName => Settings.Default.FirstName;
        public string CurrentDate => DateTime.Now.ToString("MMMM dd, yyyy");

        public Dashboard()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Dashboard_Loaded(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine($"Dashboard ActualWidth: {this.ActualWidth}, ActualHeight: {this.ActualHeight}");
            Debug.WriteLine($"DashboardMainContainer ActualWidth: {DashboardMainContainer.ActualWidth}, ActualHeight: {DashboardMainContainer.ActualHeight}");
        }
    }
}
 