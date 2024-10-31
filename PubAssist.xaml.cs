using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;

namespace IDT2025
{
    public class ProfilesDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public Microsoft.EntityFrameworkCore.DbSet<Profile> Profiles { get; set; } = null!; // Initialize to avoid CS8618

        protected override void OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite(@"Data Source=Y:\idt\InfoDevTools.db");
        }
    }

    public class Profile
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Target { get; set; } = string.Empty;
        //public string Project { get; set; } = string.Empty;
        //public string Ditamap { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
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

        private async void AddUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            string name = ProfileDetailsName.Text;
            string source = ProfileDetailsSource.Text;
            string target = ProfileDetailsTarget.Text;
            string owner = ProfileDetailsOwner.Text;

            // Use the same connection string as the Dashboard Recent Publications
            string connectionString = @"Data Source=Y:\idt\InfoDevTools.db;Version=3;";

            Debug.WriteLine("AddUpdateButton_Click: Starting database operation");
            Debug.WriteLine($"Name: {name}, Source: {source}, Target: {target}, Owner: {owner}");
            Debug.WriteLine($"Connection String: {connectionString}");

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    Debug.WriteLine("Opening SQLite connection");
                    connection.Open();
                    Debug.WriteLine("SQLite connection opened successfully");

                    // Create table if it does not exist
                    string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS Profiles (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL UNIQUE,
                        Source TEXT NOT NULL,
                        Target TEXT NOT NULL,
                        Owner TEXT NOT NULL
                    )";
                    Debug.WriteLine($"Create Table Query: {createTableQuery}");

                    using (SQLiteCommand createTableCommand = new SQLiteCommand(createTableQuery, connection))
                    {
                        Debug.WriteLine("Executing create table command");
                        createTableCommand.ExecuteNonQuery();
                        Debug.WriteLine("Create table command executed successfully");
                    }

                    string insertQuery = "INSERT INTO Profiles (Name, Source, Target, Owner) VALUES (@Name, @Source, @Target, @Owner)";
                    Debug.WriteLine($"SQL Query: {insertQuery}");

                    using (SQLiteCommand insertCommand = new SQLiteCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Name", name);
                        insertCommand.Parameters.AddWithValue("@Source", source);
                        insertCommand.Parameters.AddWithValue("@Target", target);
                        insertCommand.Parameters.AddWithValue("@Owner", owner);

                        Debug.WriteLine("Executing SQL command");
                        insertCommand.ExecuteNonQuery();
                        Debug.WriteLine("SQL command executed successfully");
                    }
                }

                MessageBox.Show("Profile details have been added/updated successfully.");

                // Refresh the ProfilesListView
                await LoadProfilesAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex.Message}");
                MessageBox.Show($"Error: {ex.Message}");
            }
        }


        private void ProfilesListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProfilesListView.SelectedItem is Profile selectedProfile)
            {
                ProfileDetailsName.Text = selectedProfile.Name;
                ProfileDetailsSource.Text = selectedProfile.Source;
                ProfileDetailsTarget.Text = selectedProfile.Target;
                ProfileDetailsOwner.Text = selectedProfile.Owner;
            }
        }

        private void SourceBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                CheckFileExists = false,
                FileName = "Select Folder",
                Filter = "Folders|no.files",
                Title = "Select Folder"
            };

            if (dialog.ShowDialog() == true)
            {
                string? folderPath = Path.GetDirectoryName(dialog.FileName);
                if (folderPath != null)
                {
                    ProfileDetailsSource.Text = folderPath;
                }
                else
                {
                    MessageBox.Show("Invalid folder path selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void TargetBrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                CheckFileExists = false,
                FileName = "Select Folder",
                Filter = "Folders|no.files",
                Title = "Select Folder"
            };

            if (dialog.ShowDialog() == true)
            {
                string? folderPath = Path.GetDirectoryName(dialog.FileName);
                if (folderPath != null)
                {
                    ProfileDetailsTarget.Text = folderPath;
                }
                else
                {
                    MessageBox.Show("Invalid folder path selected.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async Task LoadProfilesAsync()
        {
            string connectionString = @"Data Source=Y:\idt\InfoDevTools.db;Version=3;";

            try
            {
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT Name, Source, Target, Owner FROM Profiles";

                    using (SQLiteCommand command = new SQLiteCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var profiles = new List<Profile>();

                            while (await reader.ReadAsync())
                            {
                                profiles.Add(new Profile
                                {
                                    Name = reader.IsDBNull(reader.GetOrdinal("Name")) ? string.Empty : reader.GetString(reader.GetOrdinal("Name")),
                                    Source = reader.IsDBNull(reader.GetOrdinal("Source")) ? string.Empty : reader.GetString(reader.GetOrdinal("Source")),
                                    Target = reader.IsDBNull(reader.GetOrdinal("Target")) ? string.Empty : reader.GetString(reader.GetOrdinal("Target")),
                                    Owner = reader.IsDBNull(reader.GetOrdinal("Owner")) ? string.Empty : reader.GetString(reader.GetOrdinal("Owner")),
                                });
                            }

                            ProfilesListView.ItemsSource = profiles;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading profiles: {ex.Message}");
            }
        }


        private void ProfileToolsButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("ProfileToolsButton_Click: Button clicked");

            if (ProfileToolsButton.ContextMenu != null)
            {
                Debug.WriteLine("ProfileToolsButton_Click: ContextMenu is not null");
                ProfileToolsButton.ContextMenu.PlacementTarget = ProfileToolsButton;
                ProfileToolsButton.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                ProfileToolsButton.ContextMenu.IsOpen = true;
                Debug.WriteLine("ProfileToolsButton_Click: ContextMenu opened");
            }
            else
            {
                Debug.WriteLine("ProfileToolsButton_Click: ContextMenu is null");
            }
        }

        private void AddNewProfile_Click(object sender, RoutedEventArgs e)
        {
            // Add logic to add a new profile
        }

        private void DuplicateProfile_Click(object sender, RoutedEventArgs e)
        {
            // Add logic to duplicate a profile
        }

        private void DeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            // Get the selected profile
            var selectedProfile = ProfilesListView.SelectedItem as Profile;

            if (selectedProfile == null)
            {
                MessageBox.Show("Please select a profile to delete.", "No Profile Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Show confirmation message
            var result = MessageBox.Show($"Are you sure you want to delete the profile '{selectedProfile.Name}'?", "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    // Delete the profile from the database based on Name
                    using (var context = new ProfilesDbContext())
                    {
                        var profileToDelete = context.Profiles.SingleOrDefault(p => p.Name == selectedProfile.Name);
                        if (profileToDelete != null)
                        {
                            context.Profiles.Remove(profileToDelete);
                            context.SaveChanges();
                            Debug.WriteLine($"Profile '{selectedProfile.Name}' deleted from the database.");
                        }
                        else
                        {
                            Debug.WriteLine("Profile to delete not found in the database.");
                        }
                    }

                    // Refresh the ProfilesListView
                    _ = LoadProfilesAsync();
                    Debug.WriteLine("ProfilesListView refreshed.");

                    // Show success message
                    MessageBox.Show($"Profile '{selectedProfile.Name}' has been successfully deleted.", "Profile Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error occurred while deleting the profile: {ex.Message}");
                    MessageBox.Show($"An error occurred while deleting the profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }








    }
}
