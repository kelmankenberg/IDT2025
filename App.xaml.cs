using System;
using System.Security.Principal;
using System.Windows;
using IDT2025.Properties;

namespace IDT2025
{
    public partial class App : Application
    {
        public static string UserName { get; private set; }
        public static string FirstName { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            LoadUserSettings();
        }

        private void LoadUserSettings()
        {
            // Load settings
            UserName = Settings.Default.UserName;
            FirstName = Settings.Default.FirstName;

            // If settings are empty, fetch and save them
            if (string.IsNullOrEmpty(UserName))
            {
                UserName = WindowsIdentity.GetCurrent().Name;
                FirstName = ExtractFirstName(UserName);
                SaveUserSettings();
            }
        }

        private void SaveUserSettings()
        {
            // Save settings
            Settings.Default.UserName = UserName;
            Settings.Default.FirstName = FirstName;
            Settings.Default.Save();
        }

        private static string ExtractFirstName(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return string.Empty;

            var parts = userName.Split('\\');
            if (parts.Length > 1)
            {
                var nameParts = parts[1].Split('.');
                if (nameParts.Length > 0)
                {
                    return nameParts[0];
                }
            }
            return string.Empty;
        }
    }
}
