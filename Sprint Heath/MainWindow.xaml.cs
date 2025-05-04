using System;
using static Statistic_functions.DataTools;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO;
using Statistic_functions;


namespace Sprint_Heath
{
    public partial class MainWindow : Window
    {
        private static int filesCatched = 0;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenSprintSelectionWindow()
        {
            SelectionWindow sprintSelectionWindow = new SelectionWindow();
            sprintSelectionWindow.Left = this.Left;
            sprintSelectionWindow.Top = this.Top;
            sprintSelectionWindow.Show();
            this.Close();

        }

        private void DropArea_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (filePaths.Length > 0)
                {
                    string filePath = filePaths[0];
                    string fileName = System.IO.Path.GetFileName(filePath);
                    string targetDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FilesTaken");
                    if (!Directory.Exists(targetDirectory))
                    {
                        Directory.CreateDirectory(targetDirectory);
                    }
                    string destinationDirectory = System.IO.Path.Combine(targetDirectory, fileName);
                    System.IO.File.Copy(filePath, destinationDirectory, true);
                    Border dropArea = sender as Border;

                    if (dropArea != null)
                    {
                        if (dropArea.Name == "DropArea_sprints")
                        {
                            SprintsFileDropText.Text = "taken";
                            SprintsFileDropText.Foreground = Brushes.Green;
                            dropArea.AllowDrop = false;
                            DataTools.ConvertToSprints(DataTools.ExtractData(destinationDirectory));

                        }
                        else if (dropArea.Name == "DropArea_database")
                        {
                            DataFileDropText.Text = "taken";
                            DataFileDropText.Foreground = Brushes.Green;
                            dropArea.AllowDrop = false;
                            DataTools.ConvertToEvents(DataTools.ExtractData(destinationDirectory));
                        }
                        filesCatched++;
                        if (filesCatched == 2)
                        {
                            DataTools.CreateCompleteDatabase();
                            OpenSprintSelectionWindow();
                        }
                    }
                }
            }
        }
    }
}
