using ScottPlot;
using Statistic_functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Color = ScottPlot.Color;
using Colors = ScottPlot.Colors;
using static Statistic_functions.DataTools;

namespace Sprint_Heath
{
    public partial class StatisticsWindow : Window
    {
        public StatisticsWindow(DataTools.Sprint sprint)
        {
            try
            {
                InitializeComponent();
                SprintTitle.Text = sprint.name;
                var RWF = DataTools.ReadyWorkFinished(DataTools.completeDatabase[sprint], sprint.start, sprint.end);
                Dictionary<DateTime, int[]> RWFstatistics = RWF.Item1;
                int[] total = RWF.Item2;

                DataTools.AnalyzeDistribution(RWFstatistics);

                if (DataTools.parameter1 == true)
                {
                    OutputTextBlock1.Foreground = Brushes.DarkOliveGreen;
                    OutputTextBlock1.Text = "The variance of the number of events that occurred during" +
                        " the sprint for each day did not exceed the number of all events in total.\n";
                }
                else
                {
                    OutputTextBlock1.Foreground = Brushes.Firebrick;
                    OutputTextBlock1.Text = "The variance of the number of events that occurred during" +
                        " the sprint for each day exceeded the number of all events in total.\n";
                }

                if (DataTools.parameter2 == true)
                {
                    OutputTExtblock2.Foreground = Brushes.DarkOliveGreen;
                    OutputTExtblock2.Text = "The number of events for any day did not exceed the limits.\n";
                }
                else
                {
                    OutputTExtblock2.Foreground = Brushes.Firebrick;
                    OutputTExtblock2.Text = "The number of events for some days exceeded the limits.\n";
                }

                if (DataTools.parameter3 == true)
                {
                    OutputTExtblock3.Foreground = Brushes.DarkOliveGreen;
                    OutputTExtblock3.Text = "The lower limit is greater than or equal to zero.\n";
                }
                else
                {
                    OutputTExtblock3.Foreground = Brushes.Firebrick;
                    OutputTExtblock3.Text = "The lower limit is under zero.\n";
                }

                if (DataTools.parameter4 == true)
                {
                    OutputTExtblock4.Foreground = Brushes.DarkOliveGreen;
                    OutputTExtblock4.Text = "The sum of the values of none of the contracts did not exceed 29% of all" +
                        " events during the sprint.\n";
                }
                else
                {
                    OutputTExtblock4.Foreground = Brushes.Firebrick;
                    OutputTExtblock4.Text = "The sum of the values of none of the contracts exceeded 29% of all" +
                        " events during the sprint.\n";
                }


                    PixelPadding padding = new PixelPadding(60, 0, 130, 0);
                MyPlot.Plot.Layout.Fixed(padding);

                MyPlot.Plot.FigureBackground.Color = Colors.White;
                MyPlot.Plot.DataBackground.Color = Colors.White;
                Color[] categoryColors = { Colors.CadetBlue, Colors.Gray, Colors.DarkRed, Colors.DarkOliveGreen };
                string[] categoryNames = { "Initialized", "In Progress", "Failed", "Finished" };
                Tick[] ticks = new Tick[RWFstatistics.Keys.Count];
                int globalIterator = 0;

                try
                {
                    ScottPlot.Fonts.AddFontFile("Oswald", "\\Fonts\\Oswald-Light.ttf");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при добавлении шрифта: {ex.Message}");
                }

                foreach (DateTime date in RWFstatistics.Keys)
                {
                    int nextBarBase = 0;
                    int localIterator = 0;
                    foreach (int value in RWFstatistics[date])
                    {
                        try
                        {
                            ScottPlot.Bar bar = new ScottPlot.Bar()
                            {
                                Value = nextBarBase + value,
                                FillColor = categoryColors[localIterator],
                                ValueBase = nextBarBase,
                                Position = globalIterator,
                            };

                            MyPlot.Plot.Add.Bar(bar);
                            nextBarBase += value;
                            localIterator++;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка в строке {localIterator} метода 'foreach' при добавлении бара: {ex.Message}");
                        }
                    }

                    ticks[globalIterator] = new Tick(globalIterator, date.Date.ToString("dd.MM.yyyy"));
                    globalIterator++;
                }

                ScottPlot.TickGenerators.NumericManual tickGen = new ScottPlot.TickGenerators.NumericManual();
                foreach (Tick tick in ticks)
                {
                    tickGen.Add(tick);
                }
                MyPlot.Plot.Axes.Bottom.TickGenerator = tickGen;

                for (int i = 0; i < 4; i++)
                {
                    try
                    {
                        LegendItem item = new LegendItem()
                        {
                            LabelText = categoryNames[i],
                            FillColor = categoryColors[i]
                        };
                        MyPlot.Plot.Legend.ManualItems.Add(item);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка в строке {i} метода 'for' при добавлении элемента легенды: {ex.Message}");
                    }
                }

                MyPlot.Plot.Legend.Orientation = ScottPlot.Orientation.Horizontal;
                MyPlot.Plot.ShowLegend(Alignment.UpperRight);
                MyPlot.Plot.Legend.Alignment = Alignment.UpperCenter;
                MyPlot.Plot.Legend.OutlineColor = Colors.Navy;
                MyPlot.Plot.Legend.OutlineWidth = 5;
                MyPlot.Plot.Legend.BackgroundColor = Colors.White;
                MyPlot.Plot.Legend.ShadowColor = Colors.Blue.WithOpacity(.2);
                MyPlot.Plot.Legend.ShadowOffset = new PixelOffset(10, 10);
                MyPlot.Plot.Legend.FontName = "Oswald";
                MyPlot.Plot.Axes.Bottom.TickLabelStyle.FontName = "Oswald";
                MyPlot.Plot.Axes.Bottom.TickLabelStyle.FontSize = 32;
                MyPlot.Plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleCenter;
                MyPlot.Plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
                MyPlot.Plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;
                MyPlot.Plot.Axes.Left.TickLabelStyle.FontName = "Oswald";
                MyPlot.Plot.Axes.Left.TickLabelStyle.FontSize = 32;

                MyPlot.Plot.Axes.Margins(bottom: 0, top: .3);
                MyPlot.Plot.Add.HorizontalLine(DataTools.avarageOfEvents + DataTools.standardDeviationOfEvents * 2, 2, Colors.OrangeRed);
                MyPlot.Plot.Add.HorizontalLine(DataTools.avarageOfEvents - DataTools.standardDeviationOfEvents * 2, 2, Colors.OrangeRed);
                MyPlot.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в методе 'SprintStatisticsWindow': {ex.Message}");
            }
        }

        private void PreviousWindow(object sender, EventArgs e)
        {
            try
            {
                SelectionWindow sprintSelectionWindow = new SelectionWindow();
                sprintSelectionWindow.Left = this.Left;
                sprintSelectionWindow.Top = this.Top;
                sprintSelectionWindow.Show();
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка в методе 'PreviousWindow': {ex.Message}");
            }
        }

        private void CallMessageBox(object sender, EventArgs e)
        {
            MessageBox.Show(DataTools.reportation);
        }
    }
}
