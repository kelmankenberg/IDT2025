using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace IDT2025
{
    public class Profile
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public string Server { get; set; }
        public string Project { get; set; }
        public string Ditamap { get; set; }
    }

    public class ProfilesData
    {
        public List<Profile> Profile { get; set; } = new List<Profile>();
    }

    public partial class PubAssist : UserControl
    {
        public PubAssist()
        {
            InitializeComponent();
            _ = LoadProfilesAsync();
        }

        private async Task LoadProfilesAsync()
        {
            string jsonFilePath = @"\\zusscgrcprodwebhelp.file.core.windows.net\webhelp\idt\Profiles\profiles.json";
            try
            {
                if (!File.Exists(jsonFilePath))
                {
                    MessageBox.Show($"File not found: {jsonFilePath}");
                    return;
                }

                var jsonString = await File.ReadAllTextAsync(jsonFilePath);
                var profilesData = JsonSerializer.Deserialize<ProfilesData>(jsonString);
                if (profilesData?.Profile != null)
                {
                    ProfilesListView.ItemsSource = profilesData.Profile;
                }
                else
                {
                    MessageBox.Show("No profiles found in the JSON file.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading profiles: {ex.Message}");
            }
        }
    }
}
