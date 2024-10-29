using System.Diagnostics;
using System.Management;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;

namespace IDT2025
{
    public partial class MainWindow : Window
    {
        private int toggleState = 0;
        private const double SidebarWidth = 150;
        private MainViewModel _viewModel;
        private DispatcherTimer _vpnCheckTimer;
        public string AppVersion { get; set; }
        private readonly string currentVersion = "";

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            this.Icon = new BitmapImage(new Uri("pack://application:,,,/images/IDT.ico"));
            currentVersion = GetFileVersion();
            TitleVersionValue.Text = $"  v{currentVersion}";
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

            CheckForUpdatesAsync(); // Call CheckForUpdatesAsync when the window is loaded
            StartUpdateCheck();
        }

        private string GetFileVersion()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return fileVersionInfo.FileVersion;
        }

        private void StartUpdateCheck()
        {
            var updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromHours(1) // Check every hour
            };
            updateTimer.Tick += async (sender, e) => await CheckForUpdatesAsync();
            updateTimer.Start();
        }

        private async Task CheckForUpdatesAsync()
        {
            // Debug.WriteLine("Starting CheckForUpdatesAsync...");
            string updateUrl = "https://dev-documentation.wolterskluwerfs.com/IDT/update.json";
            try
            {
                var handler = new HttpClientHandler
                {
                    UseDefaultCredentials = true // Use the current user's Windows credentials
                };

                using HttpClient client = new HttpClient(handler);

                // Debug.WriteLine("Sending HTTP GET request...");
                HttpResponseMessage response = await client.GetAsync(updateUrl);
                // Debug.WriteLine($"HTTP response received. Status code: {response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    string updateInfo = await response.Content.ReadAsStringAsync();
                    // Debug.WriteLine($"Update info received: {updateInfo}");

                    if (!string.IsNullOrEmpty(updateInfo))
                    {
                        try
                        {
                            // Debug.WriteLine("Attempting to deserialize JSON...");
                            var updateData = System.Text.Json.JsonSerializer.Deserialize<UpdateInfo>(updateInfo);
                            // Debug.WriteLine("JSON deserialization successful.");

                            if (updateData != null)
                            {
                                // Debug.WriteLine($"Latest version available: {updateData.LatestVersion}");
                                //MessageBox.Show($"Latest version available: {updateData.LatestVersion}", "Update Check", MessageBoxButton.OK, MessageBoxImage.Information);

                                if (IsNewVersionAvailable(updateData.LatestVersion))
                                {
                                    // Debug.WriteLine("New version is available. Notifying user...");
                                    NotifyUserOfUpdate(updateData);
                                }
                                else
                                {
                                    // Debug.WriteLine("No new version available.");
                                }
                            }
                            else
                            {
                                // Debug.WriteLine("Deserialized update data is null.");
                                MessageBox.Show("Failed to parse update information.", "Update Check", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        catch (System.Text.Json.JsonException jsonEx)
                        {
                            // Debug.WriteLine($"JSON deserialization error: {jsonEx.Message}");
                            MessageBox.Show($"JSON deserialization error: {jsonEx.Message}", "Update Check", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                    {
                        // Debug.WriteLine("Update information is empty.");
                        MessageBox.Show("Update information is empty.", "Update Check", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    // Debug.WriteLine($"Failed to check for updates. HTTP Status: {response.StatusCode} - {response.ReasonPhrase}");
                    MessageBox.Show($"Failed to check for updates. HTTP Status: {response.StatusCode} - {response.ReasonPhrase}. URL: {updateUrl}\n\nPlease check your VPN connection and try again.", "Update Check", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                // Debug.WriteLine($"Error checking for updates: {ex.Message}");
                MessageBox.Show($"Error checking for updates: {ex.Message}. URL: {updateUrl}", "Update Check", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            // Debug.WriteLine("CheckForUpdatesAsync completed.");
        }



        private bool IsNewVersionAvailable(string latestVersion)
        {
            Version current = new Version(currentVersion);
            Version latest = new Version(latestVersion);
            return latest > current;
        }

        private void NotifyUserOfUpdate(UpdateInfo updateData)
        {
            MessageBoxResult result = MessageBox.Show(
                $"A new version ({updateData.LatestVersion}) is available. Would you like to download it?",
                "Update Available",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = updateData.DownloadUrl,
                    UseShellExecute = true
                });
            }
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
                case 0: // Sidebar open
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

                    // Change ToggleNavButton Kind to ArrowLeftBox
                    if (ToggleNavButton != null)
                    {
                        ToggleNavButton.Kind = PackIconKind.ArrowLeftBox;
                    }
                    break;

                case 1: // Sidebar partially closed
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
                case 2: // Sidebar closed
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

                    // Change ToggleNavButton Kind to ArrowRightBox
                    if (ToggleNavButton != null)
                    {
                        ToggleNavButton.Kind = PackIconKind.ArrowRightBox;
                    }
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
            VpnIcon.ToolTip = "VPN Disconnected"; // Set the default tooltip
            TopVpnIcon.ToolTip = "VPN Disconnected"; // Set the default tooltip
            VpnIconCell.ToolTip = "VPN Disconnected"; // Set the default tooltip

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
                    VpnIcon.ToolTip = "VPN Connected"; // Update the tooltip
                    TopVpnIcon.ToolTip = "VPN Connected"; // Update the tooltip
                    VpnIconCell.ToolTip = "VPN Connected"; // Update the tooltip
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
            var vpnIcon = MainGrid.FindName("VpnIcon") as PackIcon;
            var topVpnIcon = MainGrid.FindName("TopVpnIcon") as PackIcon;
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

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            if (HelpButton.ContextMenu != null)
            {
                HelpButton.ContextMenu.PlacementTarget = HelpButton;
                HelpButton.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                HelpButton.ContextMenu.IsOpen = true;
            }
        }

        private void ViewHelp_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("View Help clicked.");
        }

        private void ReleaseNotes_Click(object sender, RoutedEventArgs e)
        {
            string url = "https://dev-documentation.wolterskluwerfs.com/IDT/ReleaseNotes/index.html";
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("About IDT2025 clicked.");
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

    class UpdateInfo
    {
        [JsonPropertyName("latestVersion")]
        public string LatestVersion { get; set; }

        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonPropertyName("releaseNotes")]
        public string ReleaseNotes { get; set; }
    }
}
