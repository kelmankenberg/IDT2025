using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace IDT2025
{
    public class Profile
    {
        public string Name { get; set; } = string.Empty;
        public string Source { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public string Project { get; set; } = string.Empty;
        public string Ditamap { get; set; } = string.Empty;
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
                            Name TEXT NOT NULL,
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
                ProfileDetailsTarget.Text = selectedProfile.Server;
                ProfileDetailsOwner.Text = selectedProfile.Owner;
            }
        }

        // Update the SourceBrowseButton_Click method
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
                string folderPath = Path.GetDirectoryName(dialog.FileName);
                ProfileDetailsSource.Text = folderPath;
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
                string folderPath = Path.GetDirectoryName(dialog.FileName);
                ProfileDetailsTarget.Text = folderPath;
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
                                    Name = reader["Name"]?.ToString() ?? string.Empty,
                                    Source = reader["Source"]?.ToString() ?? string.Empty,
                                    Server = reader["Target"]?.ToString() ?? string.Empty,
                                    Owner = reader["Owner"]?.ToString() ?? string.Empty
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
            // Add logic to delete a profile
        }
    }
}
