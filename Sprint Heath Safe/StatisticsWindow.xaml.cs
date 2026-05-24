using ScottPlot;
using Statistic_functions;
using System;
using System.Collections.Generic;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Media;
using Color = ScottPlot.Color;
using Colors = ScottPlot.Colors;

namespace Sprint_Heath
{
    [SupportedOSPlatform("windows7.0")]
    public partial class StatisticsWindow : Window
    {
        private readonly DataTools.Sprint _sprint;
        private DataTools.DistributionAnalysis _analysis = DataTools.DistributionAnalysis.Empty(string.Empty);

        public StatisticsWindow(DataTools.Sprint sprint)
        {
            _sprint = sprint;
            InitializeComponent();
            RenderSprint();
        }

        private void RenderSprint()
        {
            if (!DataTools.CompleteDatabase.TryGetValue(_sprint, out List<DataTools.EventRecord>? sprintEvents))
            {
                MessageBox.Show("Для выбранного спринта не удалось собрать события.", "SprintHealth");
                return;
            }

            SprintTitle.Text = _sprint.Name;
            var (dailyStatistics, _) = DataTools.ReadyWorkFinished(sprintEvents, _sprint.Start, _sprint.End);
            _analysis = DataTools.AnalyzeDistribution(dailyStatistics);

            ApplyAnalysisTexts(_analysis);
            ConfigurePlot(dailyStatistics, _analysis);
        }

        private void ApplyAnalysisTexts(DataTools.DistributionAnalysis analysis)
        {
            SetOutputMessage(
                OutputTextBlock1,
                analysis.VarianceIsAcceptable,
                "Дисперсия количества событий по дням не превышает общее число событий спринта.",
                "Дисперсия количества событий по дням слишком велика относительно общего числа событий спринта.");

            SetOutputMessage(
                OutputTextBlock2,
                analysis.ValuesAreInsideLimits,
                "Дневные значения не выходят за расчетные границы M ± 2σ.",
                "Есть дни, в которых число событий выходит за расчетные границы M ± 2σ.");

            SetOutputMessage(
                OutputTextBlock3,
                analysis.LowerLimitIsPositive,
                "Нижняя расчетная граница не уходит в отрицательную область.",
                "Нижняя расчетная граница уходит ниже нуля, что указывает на нестабильность распределения.");

            SetOutputMessage(
                OutputTextBlock4,
                analysis.RowBalanceIsAcceptable,
                "Сумма событий ни в одном подряде не превышает 29% от общего числа событий спринта.",
                "В одном или нескольких подрядах накапливается более 29% всех событий спринта.");
        }

        private static void SetOutputMessage(System.Windows.Controls.TextBlock target, bool isPositive, string positiveText, string negativeText)
        {
            target.Foreground = isPositive ? Brushes.DarkOliveGreen : Brushes.Firebrick;
            target.Text = isPositive ? positiveText : negativeText;
        }

        private void ConfigurePlot(IReadOnlyDictionary<DateTime, int[]> dailyStatistics, DataTools.DistributionAnalysis analysis)
        {
            SprintPlot.Plot.Clear();
            SprintPlot.Plot.Layout.Fixed(new PixelPadding(80, 20, 120, 20));
            SprintPlot.Plot.FigureBackground.Color = Colors.White;
            SprintPlot.Plot.DataBackground.Color = Colors.White;
            SprintPlot.Plot.Axes.Margins(bottom: 0, top: .2);

            Color[] categoryColors =
            {
                Colors.CadetBlue,
                Colors.Gray,
                Colors.DarkOliveGreen,
                Colors.DarkRed
            };

            string[] categoryNames =
            {
                "Initialized",
                "In Progress",
                "Finished",
                "Failed"
            };

            Tick[] ticks = new Tick[dailyStatistics.Count];
            int position = 0;

            foreach ((DateTime date, int[] values) in dailyStatistics)
            {
                int nextBarBase = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    // ScottPlot v5 строит стек через ValueBase + Value, поэтому каждый следующий сегмент
                    // начинается от накопленной высоты предыдущих сегментов этого дня.
                    ScottPlot.Bar bar = new()
                    {
                        Position = position,
                        ValueBase = nextBarBase,
                        Value = nextBarBase + values[i],
                        FillColor = categoryColors[i]
                    };

                    SprintPlot.Plot.Add.Bar(bar);
                    nextBarBase += values[i];
                }

                ticks[position] = new Tick(position, date.ToString("dd.MM.yyyy"));
                position++;
            }

            ScottPlot.TickGenerators.NumericManual tickGenerator = new();
            foreach (Tick tick in ticks)
            {
                tickGenerator.Add(tick);
            }

            SprintPlot.Plot.Axes.Bottom.TickGenerator = tickGenerator;
            SprintPlot.Plot.Axes.Bottom.TickLabelStyle.FontSize = 18;
            SprintPlot.Plot.Axes.Bottom.TickLabelStyle.Rotation = 40;
            SprintPlot.Plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleLeft;
            SprintPlot.Plot.Axes.Left.TickLabelStyle.FontSize = 18;

            SprintPlot.Plot.Legend.ManualItems.Clear();
            for (int i = 0; i < categoryNames.Length; i++)
            {
                SprintPlot.Plot.Legend.ManualItems.Add(new LegendItem
                {
                    LabelText = categoryNames[i],
                    FillColor = categoryColors[i]
                });
            }

            SprintPlot.Plot.Legend.Orientation = Orientation.Horizontal;
            SprintPlot.Plot.Legend.Alignment = Alignment.UpperCenter;
            SprintPlot.Plot.Legend.BackgroundColor = Colors.White;
            SprintPlot.Plot.Legend.OutlineColor = Colors.LightGray;
            SprintPlot.Plot.ShowLegend(Alignment.UpperCenter);

            SprintPlot.Plot.Add.HorizontalLine(analysis.UpperLimit, 2, Colors.OrangeRed);
            SprintPlot.Plot.Add.HorizontalLine(analysis.LowerLimit, 2, Colors.OrangeRed);
            SprintPlot.Refresh();
        }

        private void PreviousWindow(object sender, RoutedEventArgs e)
        {
            SelectionWindow sprintSelectionWindow = new();
            sprintSelectionWindow.Left = Left;
            sprintSelectionWindow.Top = Top;
            sprintSelectionWindow.Show();
            Close();
        }

        private void CallMessageBox(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(_analysis.Report, "Ход расчетов");
        }
    }
}
