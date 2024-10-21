using IDT2025.Models;
using IDT2025.Properties;
using System.Collections.Generic;
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

namespace IDT2025
{
    public partial class Dashboard : UserControl
    {
        private GridViewColumnHeader? _lastHeaderClicked = null;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            // Debug.WriteLine("GridViewColumnHeader_Click called");

            var headerClicked = e.OriginalSource as GridViewColumnHeader;
            ListSortDirection direction;

            if (headerClicked != null)
            {
                // Debug.WriteLine($"Header clicked: {headerClicked.Content}");

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
                    // Debug.WriteLine($"Sorting by: {sortBy}, Direction: {direction}");
                    Sort(sortBy, direction);
                    _lastHeaderClicked = headerClicked;
                    _lastDirection = direction;
                }
            }
        }

        private void Sort(string sortBy, ListSortDirection direction)
        {
            // Debug.WriteLine($"Sort called with sortBy: {sortBy}, direction: {direction}");

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
            // Debug.WriteLine($"Dashboard ActualWidth: {this.ActualWidth}, ActualHeight: {this.ActualHeight}");
            // Debug.WriteLine($"DashboardMainContainer ActualWidth: {DashboardMainContainer.ActualWidth}, ActualHeight: {DashboardMainContainer.ActualHeight}");

            await LoadRecentPublicationsAsync();
        }

        private async Task LoadRecentPublicationsAsync()
        {
            string fileName = $"{FirstName}_recpubs.json";
            string filePath = $"\\\\zusscgrcprodwebhelp.file.core.windows.net\\webhelp\\idt\\RecPubs\\{fileName}";

            try
            {
                // Debug.WriteLine($"Attempting to read file from: {filePath}");

                // Read the file content directly from the file system
                var response = await File.ReadAllTextAsync(filePath);
                // Debug.WriteLine($"File content: {response}");

                // Log the JSON content for debugging
                // Debug.WriteLine("JSON Content:");
                // Debug.WriteLine(response);

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var recentPublicationsWrapper = JsonSerializer.Deserialize<RecentPublicationsWrapper>(response, options);
                var recentPublications = recentPublicationsWrapper?.Pubs?.Pub;

                if (recentPublications == null || recentPublications.Count == 0)
                {
                    // Debug.WriteLine("No data found in the JSON file.");
                    txtNumberOfRecords.Text = "0";
                    txtPubsThisMonth.Text = "0";
                    txtAverageTime.Text = "0";
                    txtLongestTime.Text = "0";
                }
                else
                {
                    // Debug.WriteLine($"Number of records found: {recentPublications.Count}");
                    foreach (var pub in recentPublications)
                    {
                        // Debug.WriteLine($"Profile: {pub.Profile}, Date: {pub.Date}, Server: {pub.Server}, Start: {pub.Start}, End: {pub.End}, Total: {pub.Total}");
                    }

                    // Update the ListView's ItemsSource
                    RecentPubsListview.ItemsSource = recentPublications;

                    // Update the txtNumberOfRecords TextBlock with the count of records
                    txtNumberOfRecords.Text = recentPublications.Count.ToString();

                    // Filter records for the current month
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

                    // Update the txtPubsThisMonth TextBlock with the count of records for the current month
                    txtPubsThisMonth.Text = pubsThisMonth.ToString();
                    txtPubsThisYear.Text = pubsThisYear.ToString();

                    // Calculate the average and longest Total time
                    var totalTimes = recentPublications.Select(pub => double.Parse(pub.Total)).ToList();
                    var averageTime = totalTimes.Average();
                    var longestTime = totalTimes.Max();

                    // Update the txtAverageTime and txtLongestTime TextBlocks
                    txtAverageTime.Text = averageTime.ToString("F2"); // Format to 2 decimal places
                    txtLongestTime.Text = longestTime.ToString();
                }
            }
            catch (FileNotFoundException fileEx)
            {
                // Debug.WriteLine($"File not found: {fileEx.Message}");
                txtNumberOfRecords.Text = "0";
                txtPubsThisMonth.Text = "0";
                txtAverageTime.Text = "0";
                txtLongestTime.Text = "0";
            }
            catch (JsonException jsonEx)
            {
                // Debug.WriteLine($"JSON error while deserializing recent publications: {jsonEx.Message}");
                // Debug.WriteLine($"Stack Trace: {jsonEx.StackTrace}");
                txtNumberOfRecords.Text = "0";
                txtPubsThisMonth.Text = "0";
                txtAverageTime.Text = "0";
                txtLongestTime.Text = "0";
            }
            catch (Exception ex)
            {
                // Debug.WriteLine($"General error while loading recent publications: {ex.Message}");
                txtNumberOfRecords.Text = "0";
                txtPubsThisMonth.Text = "0";
                txtAverageTime.Text = "0";
                txtLongestTime.Text = "0";
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
