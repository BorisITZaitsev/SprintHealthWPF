using System;
using static Statistic_functions.DataTools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Statistic_functions;


namespace Sprint_Heath
{
    public partial class SelectionWindow : Window
    {
        public SelectionWindow()
        {
            InitializeComponent();
            int[] posintion = new int[2] { 3, 1 };
            foreach (DataTools.Sprint sprint in DataTools.sprints)
            {
                Button button = new Button();

                button.Background = new SolidColorBrush(Colors.LightBlue);
                button.Click += OnClick;
                button.BorderBrush = new SolidColorBrush(Colors.Transparent);
                button.BorderThickness = new Thickness(0);


                button.FontFamily = new FontFamily("Inder");

                Style buttonStyle = new Style(typeof(Button));

                TextBlock textBlock = new TextBlock
                {
                    Text = sprint.name,
                    Foreground = new SolidColorBrush(Color.FromRgb(98, 100, 104))
                };
                button.Content = textBlock;
                Trigger hoverTrigger = new Trigger { Property = Button.IsMouseOverProperty, Value = true };
                hoverTrigger.Setters.Add(new Setter(TextBlock.ForegroundProperty, new SolidColorBrush(Color.FromRgb(0, 112, 192))));
                buttonStyle.Triggers.Add(hoverTrigger);

                myGrid.Children.Add(button);
                Grid.SetRow(button, posintion[0]);
                Grid.SetColumn(button, posintion[1]);
                Grid.SetColumnSpan(button, 5);
                posintion[0]++;
            }
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            TextBlock textBlock = (TextBlock)button.Content;
            InirializeStatisticsWindow(textBlock.Text);
        }

        private void InirializeStatisticsWindow(string sprintName)
        {

            DataTools.Sprint sprint = DataTools.sprints[0];
            foreach (DataTools.Sprint potencialSprint in DataTools.sprints)
            {
                if (potencialSprint.name == sprintName)
                {
                    sprint = potencialSprint;
                }
            }
            StatisticsWindow sprintStatisticsWindow = new StatisticsWindow(sprint);
            sprintStatisticsWindow.Left = this.Left;
            sprintStatisticsWindow.Top = this.Top;
            sprintStatisticsWindow.Show();
            this.Close();


        }
    }
}
