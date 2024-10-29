using IDT2025.Models;
using IDT2025.Properties;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace IDT2025
{
    public class RecentPublication
    {
        public string Profile { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public string Start { get; set; } = string.Empty;
        public string End { get; set; } = string.Empty;
        public double Total { get; set; }
        public string Owner { get; set; } = string.Empty;
    }

    public partial class Dashboard : UserControl
    {
        private GridViewColumnHeader? _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                if (headerClicked != _lastHeaderClicked)
                {
                    direction = ListSortDirection.Ascending;
                }
                else
                {
                    direction = _lastDirection == ListSortDirection.Ascending
                        ? ListSortDirection.Descending
                        : ListSortDirection.Ascending;
                }

                var columnBinding = headerClicked.Column.DisplayMemberBinding as Binding;
                var sortBy = columnBinding?.Path.Path ?? headerClicked.Column.Header as string;

                if (!string.IsNullOrEmpty(sortBy))
                {
                    Sort(sortBy, direction);
                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            ICollectionView dataView = CollectionViewSource.GetDefaultView(RecentPubsListview.ItemsSource);

            dataView.SortDescriptions.Clear();
            SortDescription sd = new SortDescription(sortBy, direction);
            dataView.SortDescriptions.Add(sd);
            dataView.Refresh();
        }

        public string FirstName
        {
            get
            {
                string firstName = Settings.Default.FirstName;
                return firstName == "Kelly" ? "Kel" : firstName;
            }
        }

        public string CurrentDate => DateTime.Now.ToString("MMMM dd, yyyy");

        public Dashboard()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += Dashboard_Loaded;
        }

        private async void Dashboard_Loaded(object sender, RoutedEventArgs e)
        {
            //Debug.WriteLine("Dashboard Loaded");
            await LoadRecentPublicationsAsync();
        }

        private async Task LoadRecentPublicationsAsync()
        {
            // Use a mapped drive path
            string connectionString = @"Data Source=Y:\idt\InfoDevTools.db;Version=3;";
            //Debug.WriteLine($"Using connection string: {connectionString}");

            try
            {
                Debug.WriteLine("Opening SQLite connection...");
                using (var connection = new SQLiteConnection(connectionString))
                {
                    await connection.OpenAsync();
                    //Debug.WriteLine("SQLite connection opened.");

                    string query = "SELECT Profile, Date, Server, Start, End, Total, Owner FROM RecentPublications";
                    using (var command = new SQLiteCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var recentPublications = new List<RecentPublication>();
                        //Debug.WriteLine("Executing query...");

                        while (await reader.ReadAsync())
                        {
                            var publication = new RecentPublication
                            {
                                Profile = reader["Profile"]?.ToString() ?? string.Empty,
                                Date = reader["Date"]?.ToString() ?? string.Empty,
                                Server = reader["Server"]?.ToString() ?? string.Empty,
                                Start = DateTime.Parse(reader["Start"]?.ToString() ?? string.Empty).ToString("HH:mm:ss"),
                                End = DateTime.Parse(reader["End"]?.ToString() ?? string.Empty).ToString("HH:mm:ss"),
                                Total = reader["Total"] != DBNull.Value ? Convert.ToDouble(reader["Total"]) : 0,
                                Owner = reader["Owner"]?.ToString() ?? string.Empty
                            };
                            recentPublications.Add(publication);
                            //Debug.WriteLine($"Loaded publication: {publication.Profile}, {publication.Date}");
                        }

                        if (recentPublications.Count == 0)
                        {
                            //Debug.WriteLine("No recent publications found.");
                            txtNumberOfRecords.Text = "0";
                            txtPubsThisMonth.Text = "0";
                            txtAverageTime.Text = "0";
                            txtLongestTime.Text = "0";
                        }
                        else
                        {
                            //Debug.WriteLine($"Loaded {recentPublications.Count} publications.");
                            RecentPubsListview.ItemsSource = recentPublications;
                            txtNumberOfRecords.Text = recentPublications.Count.ToString();

                            var currentMonth = DateTime.Now.Month;
                            var currentYear = DateTime.Now.Year;
                            var pubsThisMonth = recentPublications.Count(pub =>
                            {
                                var pubDate = DateTime.Parse(pub.Date);
                                return pubDate.Month == currentMonth && pubDate.Year == currentYear;
                            });
                            var pubsThisYear = recentPublications.Count(pub =>
                            {
                                DateTime pubDate;
                                if (DateTime.TryParse(pub.Date, out pubDate))
                                {
                                    return pubDate.Year == currentYear;
                                }
                                return false;
                            });

                            txtPubsThisMonth.Text = pubsThisMonth.ToString();
                            txtPubsThisYear.Text = pubsThisYear.ToString();

                            var totalTimes = recentPublications.Select(pub => pub.Total).ToList();
                            var averageTime = totalTimes.Average();
                            var longestTime = totalTimes.Max();

                            txtAverageTime.Text = averageTime.ToString("F2");
                            txtLongestTime.Text = longestTime.ToString();
                        }
                    }
                }
            }
            catch (SQLiteException sqlEx)
            {
                Debug.WriteLine($"SQLiteException: {sqlEx.Message}");
                // Handle SQLite exceptions
                txtNumberOfRecords.Text = "0";
                txtPubsThisMonth.Text = "0";
                txtAverageTime.Text = "0";
                txtLongestTime.Text = "0";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}");
                // Handle general exceptions
                txtNumberOfRecords.Text = "0";
                txtPubsThisMonth.Text = "0";
                txtAverageTime.Text = "0";
                txtLongestTime.Text = "0";
            }
        }

        private void ImpersonateAndLoadData()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();

            try
            {
                WindowsIdentity.RunImpersonated(identity.AccessToken, async () =>
                {
                    await LoadRecentPublicationsAsync();
                }).Wait();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception during impersonation: {ex.Message}");
            }
        }
    }

    public static class GridViewColumnExtensions
    {
        public static readonly DependencyProperty AutoWidthProperty =
            DependencyProperty.RegisterAttached(
                "AutoWidth",
                typeof(bool),
                typeof(GridViewColumnExtensions),
                new PropertyMetadata(false, OnAutoWidthPropertyChanged));

        public static bool GetAutoWidth(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoWidthProperty);
        }

        public static void SetAutoWidth(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoWidthProperty, value);
        }

        private static void OnAutoWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GridViewColumn column && e.NewValue is bool)
            {
                if ((bool)e.NewValue)
                {
                    column.Width = double.NaN;
                    column.Width = column.ActualWidth;
                    column.Width = double.NaN;
                }
            }
        }
    }
}
