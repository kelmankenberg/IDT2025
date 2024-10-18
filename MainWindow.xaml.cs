using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace IDT2025
{
    public partial class MainWindow : Window
    {
        private int toggleState = 0;
        private const double SidebarWidth = 150; // Adjust this value based on your sidebar width

        public MainWindow()
        {
            InitializeComponent();
            // Set the default toggle state to case 0 on startup
            SetInitialToggleState();
        }

        private void SetInitialToggleState()
        {
            var DashboardLabelCell = MainGrid.FindName("DashboardLabelCell") as Border;
            var PubAssistLabelCell = MainGrid.FindName("PubAssistLabelCell") as Border;
            var SingleFileLabelCell = MainGrid.FindName("SingleFileLabelCell") as Border;
            var EditorLabelCell = MainGrid.FindName("EditorLabelCell") as Border;
            var SettingsLabelCell = MainGrid.FindName("SettingsLabelCell") as Border;
            var VpnLabelCell = MainGrid.FindName("VpnLabelCell") as Border;

            DashboardLabelCell.Visibility = Visibility.Visible;
            PubAssistLabelCell.Visibility = Visibility.Visible;
            SingleFileLabelCell.Visibility = Visibility.Visible;
            EditorLabelCell.Visibility = Visibility.Visible;
            SettingsLabelCell.Visibility = Visibility.Visible;
            VpnLabelCell.Visibility = Visibility.Visible;

            DashboardIconCell.Visibility = Visibility.Visible;
            PubAssistIconCell.Visibility = Visibility.Visible;
            SingleFileIconCell.Visibility = Visibility.Visible;
            EditorIconCell.Visibility = Visibility.Visible;
            SettingsIconCell.Visibility = Visibility.Visible;
            VpnIconCell.Visibility = Visibility.Visible;

            TopNav.Visibility = Visibility.Collapsed;

            // Adjust MainContent margin
            MainContent.Margin = new Thickness(20, 0, 0, 0); // Adjust the left margin as needed
        }

        private void NavToggleButton_Click(object sender, RoutedEventArgs e)
        {
            toggleState = (toggleState + 1) % 3;
            Debug.WriteLine($"Toggle state changed to {toggleState}");

            var DashboardLabelCell = MainGrid.FindName("DashboardLabelCell") as Border;
            var PubAssistLabelCell = MainGrid.FindName("PubAssistLabelCell") as Border;
            var SingleFileLabelCell = MainGrid.FindName("SingleFileLabelCell") as Border;
            var EditorLabelCell = MainGrid.FindName("EditorLabelCell") as Border;
            var SettingsLabelCell = MainGrid.FindName("SettingsLabelCell") as Border;
            var VpnLabelCell = MainGrid.FindName("VpnLabelCell") as Border;
            var IconCellWidth = 50;
            var NavLabelCellWidth = 150;

            switch (toggleState)
            {
                case 0:
                    DashboardLabelCell.Visibility = Visibility.Visible;
                    PubAssistLabelCell.Visibility = Visibility.Visible;
                    SingleFileLabelCell.Visibility = Visibility.Visible;
                    EditorLabelCell.Visibility = Visibility.Visible;
                    SettingsLabelCell.Visibility = Visibility.Visible;
                    VpnLabelCell.Visibility = Visibility.Visible;

                    DashboardIconCell.Visibility = Visibility.Visible;
                    PubAssistIconCell.Visibility = Visibility.Visible;
                    SingleFileIconCell.Visibility = Visibility.Visible;
                    EditorIconCell.Visibility = Visibility.Visible;
                    SettingsIconCell.Visibility = Visibility.Visible;
                    VpnIconCell.Visibility = Visibility.Visible;

                    TopNav.Visibility = Visibility.Collapsed;

                    // Adjust MainContent margin
                    MainContent.Margin = new Thickness(20, 0, 0, 0); // Adjust the left margin as needed
                    break;

                case 1:
                    DashboardLabelCell.Visibility = Visibility.Collapsed;
                    PubAssistLabelCell.Visibility = Visibility.Collapsed;
                    SingleFileLabelCell.Visibility = Visibility.Collapsed;
                    EditorLabelCell.Visibility = Visibility.Collapsed;
                    SettingsLabelCell.Visibility = Visibility.Collapsed;
                    VpnLabelCell.Visibility = Visibility.Collapsed;

                    DashboardIconCell.Visibility = Visibility.Visible;
                    PubAssistIconCell.Visibility = Visibility.Visible;
                    SingleFileIconCell.Visibility = Visibility.Visible;
                    EditorIconCell.Visibility = Visibility.Visible;
                    SettingsIconCell.Visibility = Visibility.Visible;
                    VpnIconCell.Visibility = Visibility.Visible;

                    TopNav.Visibility = Visibility.Collapsed;

                    // Adjust MainContent margin
                    MainContent.Margin = new Thickness(-(SidebarWidth) + 20, 0, 0, 0); // Adjust the left margin as needed
                    break;
                case 2:
                    DashboardLabelCell.Visibility = Visibility.Collapsed;
                    PubAssistLabelCell.Visibility = Visibility.Collapsed;
                    SingleFileLabelCell.Visibility = Visibility.Collapsed;
                    EditorLabelCell.Visibility = Visibility.Collapsed;
                    SettingsLabelCell.Visibility = Visibility.Collapsed;
                    VpnLabelCell.Visibility = Visibility.Collapsed;

                    DashboardIconCell.Visibility = Visibility.Collapsed;
                    PubAssistIconCell.Visibility = Visibility.Collapsed;
                    SingleFileIconCell.Visibility = Visibility.Collapsed;
                    EditorIconCell.Visibility = Visibility.Collapsed;
                    SettingsIconCell.Visibility = Visibility.Collapsed;
                    VpnIconCell.Visibility = Visibility.Collapsed;

                    TopNav.Visibility = Visibility.Visible;

                    // Adjust MainContent margin
                    MainContent.Margin = new Thickness(-200, 0, 0, 0); // Adjust the left margin as needed
                    break;
            }
        }

        private void DashboardIconCell_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainContent.Content = new Dashboard();
        }

        private void TopNavDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            DashboardIconCell_MouseLeftButtonDown(DashboardIconCell, null);
        }

        private void SetNavLabelCellVisibility(Visibility visibility)
        {
            var navLabelCellStyle = FindResource("NavLabelCell") as Style;
            if (navLabelCellStyle == null)
            {
                MessageBox.Show("NavLabelCell style not found.");
                return;
            }

            foreach (var element in MainGrid.Children)
            {
                if (element is Border border)
                {
                    Debug.WriteLine($"Border Name: {border.Name}, Style: {border.Style}");
                    if (border.Style == navLabelCellStyle)
                    {
                        border.Visibility = visibility;
                        Debug.WriteLine($"Set visibility of {border.Name} to {visibility}");
                    }
                }
            }
        }

        private void SetNavIconCellVisibility(Visibility visibility)
        {
            var navIconCellStyle = FindResource("NavIconCell") as Style;
            if (navIconCellStyle == null)
            {
                MessageBox.Show("NavIconCell style not found.");
                return;
            }

            foreach (var element in MainGrid.Children)
            {
                if (element is Border border)
                {
                    Debug.WriteLine($"Border Name: {border.Name}, Style: {border.Style}");
                    if (border.Style == navIconCellStyle)
                    {
                        border.Visibility = visibility;
                        Debug.WriteLine($"Set visibility of {border.Name} to {visibility}");
                    }
                }
            }
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                ToggleWindowState();
            }
            else
            {
                DragMove();
            }
        }

        private void ToggleWindowState()
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
            else
            {
                this.WindowState = WindowState.Maximized;
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            ToggleWindowState();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            // Handle state change if needed
        }

        private void SetDashboardLabelCellVisibility(Visibility visibility)
        {
            var dashboardLabelCell = MainGrid.FindName("DashboardLabelCell") as Border;
            if (dashboardLabelCell != null)
            {
                dashboardLabelCell.Visibility = visibility;
                Debug.WriteLine($"Set visibility of DashboardLabelCell to {visibility}");
            }
            else
            {
                MessageBox.Show("DashboardLabelCell not found.");
            }
        }
    }
}
