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

                string output1 = $"During the sprint {total[0]} tasks were initialized, " +
                    $"{total[1]} reached 'in progres' status, {total[2]} were failed " +
                    $"and {total[3]} were finished successfully.\n" + "The first criteria for a successful organisation of " +
                    "work activities is activity distributions.\nCalculation formula: " +
                    "80% of the number of events during the sprint must not appear during less " +
                    "than 71.4% of the time interval.";
                string output3 = "According to the given statistics, sprint " +
                    $"'{sprint.name}' was organized";
                string output4 = " well. Everything seems to be OK.";

                OutputTExtblock2.Foreground = Brushes.DarkOliveGreen;

                int timeIntervalLength = (int)Math.Round(RWFstatistics.Keys.Count * 0.714);
                for (int leftThreshold = 0; leftThreshold < RWFstatistics.Count - timeIntervalLength; leftThreshold++)
                {
                    int rightThreshold = leftThreshold + timeIntervalLength;
                    int intervalSumm = 0;
                    for (int i = leftThreshold; i < rightThreshold; i++)
                    {
                        try
                        {
                            DateTime key = RWFstatistics.Keys.ToArray()[i];
                            intervalSumm += RWFstatistics[key].Sum();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка в строке {i} метода 'for' при доступе к RWFstatistics: {ex.Message}");
                        }
                    }
                    if (intervalSumm > total.Sum() * 0.8)
                    {
                        output4 = $" WRONG{intervalSumm / (total.Sum() / 100)}% of" +
                            $" the tasks appeared from{RWFstatistics.Keys.ToArray()[leftThreshold].ToString("dd.MM.yyyy")} to {RWFstatistics.Keys.ToArray()[rightThreshold].ToString("dd.MM.yyyy")}.";
                        OutputTExtblock2.Foreground = Brushes.Red;
                    }
                }

                OutputTextBlock1.Text = output1;
                OutputTExtblock2.Text = output3 + output4;

                string output5 = "The second criterion for a good sprint is the distribution of events. " +
                    "If the distribution tends to be even, the sprint is considered well organised.";
                string output6 = "The distribution of the events of current sprint was uniform.";
                OutputTExtblock4.Foreground = Brushes.DarkOliveGreen;
                bool organizedWell = DataTools.AnalyzeDistribution(RWFstatistics);
                if (!organizedWell)
                {
                    output6 = "The distribution of the events of current sprint was not uniform.";
                    OutputTExtblock4.Foreground = Brushes.Red;
                }
                OutputTExtblock3.Text = output5;
                OutputTExtblock4.Text = output6;

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
                    ScottPlot.Fonts.AddFontFile("Oswald", "C:\\CSAIO4D\\BK01\\CH02\\Sprint Heath\\Fonts\\Oswald-Light.ttf");
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
                MyPlot.Plot.Legend.FontSize = 32;
                MyPlot.Plot.Legend.FontName = "Oswald";
                MyPlot.Plot.Axes.Bottom.TickLabelStyle.FontName = "Oswald";
                MyPlot.Plot.Axes.Bottom.TickLabelStyle.FontSize = 32;
                MyPlot.Plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleCenter;
                MyPlot.Plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
                MyPlot.Plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;
                MyPlot.Plot.Axes.Left.TickLabelStyle.FontName = "Oswald";
                MyPlot.Plot.Axes.Left.TickLabelStyle.FontSize = 32;

                MyPlot.Plot.Axes.Margins(bottom: 0, top: .3);
                MyPlot.Plot.Add.HorizontalLine(DataTools.avarageOfEvents + DataTools.standartstandardDeviationOfEvents, 2, Colors.OrangeRed);
                MyPlot.Plot.Add.HorizontalLine(DataTools.avarageOfEvents - DataTools.standartstandardDeviationOfEvents, 2, Colors.OrangeRed);
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
            MessageBox.Show(DataTools.additionalInformation);
        }
    }
}
