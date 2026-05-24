using Statistic_functions;
using System;
using System.IO;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sprint_Heath
{
    [SupportedOSPlatform("windows7.0")]
    public partial class MainWindow : Window
    {
        private bool _sprintsLoaded;
        private bool _eventsLoaded;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenSprintSelectionWindow()
        {
            SelectionWindow sprintSelectionWindow = new();
            sprintSelectionWindow.Left = Left;
            sprintSelectionWindow.Top = Top;
            sprintSelectionWindow.Show();
            Close();
        }

        private void DropArea_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                return;
            }

            string[] filePaths = (string[])e.Data.GetData(DataFormats.FileDrop);
            if (filePaths.Length == 0 || sender is not Border dropArea)
            {
                return;
            }

            string copiedFilePath = CopyFileToWorkingDirectory(filePaths[0]);

            if (dropArea.Name == nameof(DropArea_sprints))
            {
                DataTools.LoadSprintsFromFile(copiedFilePath);
                MarkDropAreaAsLoaded(SprintsFileDropText, dropArea);
                _sprintsLoaded = true;
            }
            else if (dropArea.Name == nameof(DropArea_database))
            {
                DataTools.LoadEventsFromFile(copiedFilePath);
                MarkDropAreaAsLoaded(DataFileDropText, dropArea);
                _eventsLoaded = true;
            }

            if (_sprintsLoaded && _eventsLoaded)
            {
                // После загрузки обоих файлов связываем события со спринтами по entityIds.
                DataTools.CreateCompleteDatabase();
                OpenSprintSelectionWindow();
            }
        }

        private static string CopyFileToWorkingDirectory(string sourcePath)
        {
            string targetDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FilesTaken");
            Directory.CreateDirectory(targetDirectory);

            string fileName = Path.GetFileName(sourcePath);
            string destinationPath = Path.Combine(targetDirectory, fileName);
            File.Copy(sourcePath, destinationPath, overwrite: true);

            return destinationPath;
        }

        private static void MarkDropAreaAsLoaded(TextBlock label, Border dropArea)
        {
            label.Text = "файл загружен";
            label.Foreground = Brushes.SeaGreen;
            dropArea.AllowDrop = false;
        }
    }
}
