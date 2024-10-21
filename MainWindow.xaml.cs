using FontAwesome.WPF;
using System.Diagnostics;
using System.Management;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace IDT2025
{
    public partial class MainWindow : Window
    {
        private int toggleState = 0;
        private const double SidebarWidth = 150;
        private MainViewModel _viewModel;
        private DispatcherTimer _vpnCheckTimer;

        public MainWindow()
        {
            InitializeComponent();
            _viewModel = new MainViewModel();
            DataContext = _viewModel;
            // Set the default toggle state to case 0 on startup
            SetInitialToggleState();
            Loaded += MainWindow_Loaded; // Move this inside the constructor

            // Initialize and start the VPN check timer
            _vpnCheckTimer = new DispatcherTimer();
            _vpnCheckTimer.Interval = TimeSpan.FromSeconds(10); // Check every 10 seconds
            _vpnCheckTimer.Tick += VpnCheckTimer_Tick;
            _vpnCheckTimer.Start();


            LoadUserSettings();

            LoadDashboard();
        }

        private void ApplyToggleState(int state)
        {
            var DashboardLabelCell = MainGrid.FindName("DashboardLabelCell") as Border;
            var PubAssistLabelCell = MainGrid.FindName("PubAssistLabelCell") as Border;
            var SingleFileLabelCell = MainGrid.FindName("SingleFileLabelCell") as Border;
            var EditorLabelCell = MainGrid.FindName("EditorLabelCell") as Border;
            var SettingsLabelCell = MainGrid.FindName("SettingsLabelCell") as Border;
            var VpnLabelCell = MainGrid.FindName("VpnLabelCell") as Border;

            switch (state)
            {
                case 0:
                    if (DashboardLabelCell != null) DashboardLabelCell.Visibility = Visibility.Visible;
                    if (PubAssistLabelCell != null) PubAssistLabelCell.Visibility = Visibility.Visible;
                    if (SingleFileLabelCell != null) SingleFileLabelCell.Visibility = Visibility.Visible;
                    if (EditorLabelCell != null) EditorLabelCell.Visibility = Visibility.Visible;
                    if (SettingsLabelCell != null) SettingsLabelCell.Visibility = Visibility.Visible;
                    if (VpnLabelCell != null) VpnLabelCell.Visibility = Visibility.Visible;

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
                    if (DashboardLabelCell != null) DashboardLabelCell.Visibility = Visibility.Collapsed;
                    if (PubAssistLabelCell != null) PubAssistLabelCell.Visibility = Visibility.Collapsed;
                    if (SingleFileLabelCell != null) SingleFileLabelCell.Visibility = Visibility.Collapsed;
                    if (EditorLabelCell != null) EditorLabelCell.Visibility = Visibility.Collapsed;
                    if (SettingsLabelCell != null) SettingsLabelCell.Visibility = Visibility.Collapsed;
                    if (VpnLabelCell != null) VpnLabelCell.Visibility = Visibility.Collapsed;

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
                    if (DashboardLabelCell != null) DashboardLabelCell.Visibility = Visibility.Collapsed;
                    if (PubAssistLabelCell != null) PubAssistLabelCell.Visibility = Visibility.Collapsed;
                    if (SingleFileLabelCell != null) SingleFileLabelCell.Visibility = Visibility.Collapsed;
                    if (EditorLabelCell != null) EditorLabelCell.Visibility = Visibility.Collapsed;
                    if (SettingsLabelCell != null) SettingsLabelCell.Visibility = Visibility.Collapsed;
                    if (VpnLabelCell != null) VpnLabelCell.Visibility = Visibility.Collapsed;

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

        private void LoadUserSettings()
        {
            // Load settings
            string userName = Properties.Settings.Default.UserName;
            toggleState = Properties.Settings.Default.SidebarToggleState; // Load the sidebar toggle state
            ApplyToggleState(toggleState); // Apply the loaded toggle state
        }

        private void SaveUserSettings()
        {
            // Modify settings
            Properties.Settings.Default.UserName = "NewUserName";
            Properties.Settings.Default.SidebarToggleState = toggleState; // Save the sidebar toggle state
                                                                          // Save settings
            Properties.Settings.Default.Save();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Save settings when the window is closing
            SaveUserSettings();
        }

        private void LoadDashboard()
        {
            MainContent.Content = new Dashboard();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            CheckVpnStatus(); // Call CheckVpnStatus when the window is loaded
        }

        private void VpnCheckTimer_Tick(object sender, EventArgs e)
        {
            CheckVpnStatus(); // Periodically check VPN status
        }

        private void CheckVpnStatus()
        {
            bool isConnected = false;

            // Use WMI to check VPN connection status
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_NetworkAdapter WHERE NetConnectionID != NULL");
            foreach (ManagementObject queryObj in searcher.Get())
            {
                var netConnectionId = queryObj["NetConnectionID"]?.ToString();
                var netConnectionStatus = queryObj["NetConnectionStatus"]?.ToString();
                var description = queryObj["Description"]?.ToString();

                //Debug.WriteLine($"NetConnectionID: {netConnectionId}, NetConnectionStatus: {netConnectionStatus}, Description: {description}");

                // Check for the specific VPN adapter description and ensure the status is connected
                if (description != null &&
                    description.Equals("Cisco AnyConnect Virtual Miniport Adapter for Windows x64", StringComparison.OrdinalIgnoreCase) &&
                    netConnectionStatus == "2")
                {
                    isConnected = true;
                    break;
                }
            }

            //Debug.WriteLine($"VPN Connected: {isConnected}");
            _viewModel.IsVpnConnected = isConnected;

            // Update the VPN icon color based on the connection status
            UpdateVpnIconColor(isConnected);
        }

        private void UpdateVpnIconColor(bool isConnected)
        {
            var vpnIcon = MainGrid.FindName("VpnIcon") as ImageAwesome;
            var topVpnIcon = MainGrid.FindName("TopVpnIcon") as ImageAwesome;
            if (vpnIcon != null)
            {
                vpnIcon.Foreground = isConnected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00cc00")) : Brushes.Red;
            }
            if (topVpnIcon != null)
            {
                topVpnIcon.Foreground = isConnected ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00cc00")) : Brushes.Red;
            }
        }


        private void SetInitialToggleState()
        {
            var DashboardLabelCell = MainGrid.FindName("DashboardLabelCell") as Border;
            var PubAssistLabelCell = MainGrid.FindName("PubAssistLabelCell") as Border;
            var SingleFileLabelCell = MainGrid.FindName("SingleFileLabelCell") as Border;
            var EditorLabelCell = MainGrid.FindName("EditorLabelCell") as Border;
            var SettingsLabelCell = MainGrid.FindName("SettingsLabelCell") as Border;
            var VpnLabelCell = MainGrid.FindName("VpnLabelCell") as Border;

            if (DashboardLabelCell != null) DashboardLabelCell.Visibility = Visibility.Visible;
            if (PubAssistLabelCell != null) PubAssistLabelCell.Visibility = Visibility.Visible;
            if (SingleFileLabelCell != null) SingleFileLabelCell.Visibility = Visibility.Visible;
            if (EditorLabelCell != null) EditorLabelCell.Visibility = Visibility.Visible;
            if (SettingsLabelCell != null) SettingsLabelCell.Visibility = Visibility.Visible;
            if (VpnLabelCell != null) VpnLabelCell.Visibility = Visibility.Visible;

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
            //Debug.WriteLine($"Toggle state changed to {toggleState}");

            ApplyToggleState(toggleState);

            SaveUserSettings(); // Save the toggle state when it changes
        }

        private void DashboardIconCell_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainContent.Content = new Dashboard();
        }

        private void PubAssistIconCell_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            MainContent.Content = new PubAssist();
        }

        private void TopNavDashboardButton_Click(object sender, RoutedEventArgs e)
        {
            DashboardIconCell_MouseLeftButtonDown(DashboardIconCell, new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = UIElement.MouseLeftButtonDownEvent });
        }

        private void TopNavPubAssistButton_Click(object sender, RoutedEventArgs e)
        {
            PubAssistIconCell_MouseLeftButtonDown(PubAssistIconCell, new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = UIElement.MouseLeftButtonDownEvent });
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
                    //Debug.WriteLine($"Border Name: {border.Name}, Style: {border.Style}");
                    if (border.Style == navLabelCellStyle)
                    {
                        border.Visibility = visibility;
                        //Debug.WriteLine($"Set visibility of {border.Name} to {visibility}");
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
                    //Debug.WriteLine($"Border Name: {border.Name}, Style: {border.Style}");
                    if (border.Style == navIconCellStyle)
                    {
                        border.Visibility = visibility;
                        //Debug.WriteLine($"Set visibility of {border.Name} to {visibility}");
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
                //Debug.WriteLine($"Set visibility of DashboardLabelCell to {visibility}");
            }
            else
            {
                MessageBox.Show("DashboardLabelCell not found.");
            }
        }
    }
}
