using System;
using System.Security.Principal;
using System.Windows;

namespace IDT2025
{
    public partial class App : Application
    {
        public static string UserName { get; private set; }
        public static string FirstName { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            UserName = WindowsIdentity.GetCurrent().Name;
            FirstName = ExtractFirstName(UserName);
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
