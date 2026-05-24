using Statistic_functions;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sprint_Heath
{
    [SupportedOSPlatform("windows7.0")]
    public partial class SelectionWindow : Window
    {
        public SelectionWindow()
        {
            InitializeComponent();

            foreach (DataTools.Sprint sprint in DataTools.Sprints)
            {
                Button button = new()
                {
                    Margin = new Thickness(0, 0, 0, 12),
                    Padding = new Thickness(18, 14, 18, 14),
                    Background = Brushes.White,
                    BorderBrush = new SolidColorBrush(Color.FromRgb(127, 179, 213)),
                    BorderThickness = new Thickness(1.5),
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Tag = sprint
                };

                button.Click += OnClick;
                button.Content = new TextBlock
                {
                    Text = sprint.Name,
                    FontSize = 18,
                    Foreground = new SolidColorBrush(Color.FromRgb(51, 78, 104)),
                    TextWrapping = TextWrapping.Wrap
                };

                SprintButtonsPanel.Children.Add(button);
            }
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is DataTools.Sprint sprint)
            {
                InitializeStatisticsWindow(sprint);
            }
        }

        private void InitializeStatisticsWindow(DataTools.Sprint sprint)
        {
            StatisticsWindow sprintStatisticsWindow = new(sprint);
            sprintStatisticsWindow.Left = Left;
            sprintStatisticsWindow.Top = Top;
            sprintStatisticsWindow.Show();
            Close();
        }
    }
}
