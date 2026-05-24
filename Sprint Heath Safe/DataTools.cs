using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Statistic_functions
{
    public static class DataTools
    {
        private static readonly HashSet<string> CompletedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Закрыто",
            "Выполнено"
        };

        private static readonly HashSet<string> FailedStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Отклонен исполнителем"
        };

        private static readonly HashSet<string> FailedResolutions = new(StringComparer.OrdinalIgnoreCase)
        {
            "Отклонено",
            "Отменено инициатором",
            "Дубликат"
        };

        public static Dictionary<Sprint, List<EventRecord>> CompleteDatabase { get; private set; } = new();

        public static List<Sprint> Sprints { get; private set; } = new();

        public static List<EventRecord> Events { get; private set; } = new();

        public static void LoadSprintsFromFile(string filePath)
        {
            ConvertToSprints(ExtractData(filePath));
        }

        public static void LoadEventsFromFile(string filePath)
        {
            ConvertToEvents(ExtractData(filePath));
        }

        public static void CreateCompleteDatabase()
        {
            var database = new Dictionary<Sprint, List<EventRecord>>();

            foreach (Sprint sprint in Sprints)
            {
                HashSet<int> sprintEntityIds = sprint.EntityIds.ToHashSet();
                List<EventRecord> sprintEvents = Events
                    .Where(evt => sprintEntityIds.Contains(evt.Id))
                    .OrderBy(evt => evt.CreationDate)
                    .ToList();

                database[sprint] = sprintEvents;
            }

            CompleteDatabase = database;
        }

        public static DistributionAnalysis AnalyzeDistribution(IReadOnlyDictionary<DateTime, int[]> statistics)
        {
            if (statistics.Count == 0)
            {
                return DistributionAnalysis.Empty("Для выбранного спринта не найдено событий, пригодных для анализа.");
            }

            int[] dailyTotals = statistics
                .OrderBy(pair => pair.Key)
                .Select(pair => pair.Value.Sum())
                .ToArray();

            int totalEvents = dailyTotals.Sum();
            double average = dailyTotals.Average();
            double variance = dailyTotals.Sum(value => Math.Pow(value - average, 2)) / dailyTotals.Length;
            double standardDeviation = Math.Sqrt(variance);
            double lowerLimit = average - standardDeviation * 2;
            double upperLimit = average + standardDeviation * 2;
            int rowLength = Math.Max(1, (int)Math.Round(dailyTotals.Length * 0.2727, MidpointRounding.AwayFromZero));

            // Подряд вычисляется скользящим окном, чтобы проверить концентрацию нагрузки внутри спринта.
            List<int> rollingSums = new();
            for (int i = 0; i < dailyTotals.Length - rowLength; i++)
            {
                rollingSums.Add(dailyTotals.Skip(i).Take(rowLength).Sum());
            }

            bool varianceIsAcceptable = variance <= totalEvents;
            bool valuesAreInsideLimits = dailyTotals.All(value => value >= lowerLimit && value <= upperLimit);
            bool lowerLimitIsPositive = lowerLimit >= 0;
            bool rowBalanceIsAcceptable = rollingSums.All(sum => sum <= totalEvents * 0.29);

            string report = string.Join(
                Environment.NewLine,
                new[]
                {
                    $"Количество событий: {totalEvents}",
                    $"Среднее значение: {average:F3}",
                    $"Дисперсия: {variance:F3}",
                    $"Среднеквадратичное отклонение: {standardDeviation:F3}",
                    $"Нижняя граница (M - 2σ): {lowerLimit:F3}",
                    $"Верхняя граница (M + 2σ): {upperLimit:F3}",
                    $"Длина подряда: {rowLength}",
                    $"Максимальная сумма подряда: {(rollingSums.Count == 0 ? 0 : rollingSums.Max())}"
                });

            return new DistributionAnalysis(
                totalEvents,
                average,
                variance,
                standardDeviation,
                lowerLimit,
                upperLimit,
                rowLength,
                varianceIsAcceptable,
                valuesAreInsideLimits,
                lowerLimitIsPositive,
                rowBalanceIsAcceptable,
                report);
        }

        public static void ConvertToEvents(List<string> data)
        {
            Events = data
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Split(';'))
                .Where(row => row.Length >= 21)
                .Select(row => new EventRecord(
                    row[0],
                    row[1],
                    row[2],
                    row[3],
                    row[4],
                    row[5],
                    row[6],
                    row[7],
                    row[8],
                    row[9],
                    row[10],
                    row[11],
                    row[12],
                    row[13],
                    row[14],
                    row[15],
                    row[16],
                    row[17],
                    row[18],
                    row[19],
                    row[20]))
                .ToList();
        }

        public static void ConvertToSprints(List<string> data)
        {
            Sprints = data
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Split(';'))
                .Where(row => row.Length >= 5)
                .Select(row => new Sprint(row[0], row[1], row[2], row[3], row[4]))
                .OrderBy(sprint => sprint.Start)
                .ToList();
        }

        public static List<string> ExtractData(string filePath)
        {
            return File.ReadAllLines(filePath, Encoding.UTF8)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();
        }

        public static (SortedDictionary<DateTime, int[]> DailyStatistics, int[] Totals) ReadyWorkFinished(
            IEnumerable<EventRecord> events,
            DateTime sprintStart,
            DateTime sprintFinish)
        {
            SortedDictionary<DateTime, int[]> dailyStatistics = new();
            int[] totals = new int[4];

            foreach (EventRecord evt in events
                         .Where(evt => evt.CreationDate != DateTime.MinValue)
                         .Where(evt => evt.CreationDate.Date >= sprintStart.Date && evt.CreationDate.Date <= sprintFinish.Date)
                         .Where(evt => !string.Equals(evt.Type, "Дефект", StringComparison.OrdinalIgnoreCase))
                         .OrderBy(evt => evt.CreationDate))
            {
                DateTime date = evt.CreationDate.Date;
                if (!dailyStatistics.TryGetValue(date, out int[]? dayTotals))
                {
                    dayTotals = new int[4];
                    dailyStatistics[date] = dayTotals;
                }

                int categoryIndex = ResolveCategoryIndex(evt);
                dayTotals[categoryIndex]++;
                totals[categoryIndex]++;
            }

            return (dailyStatistics, totals);
        }

        private static int ResolveCategoryIndex(EventRecord evt)
        {
            if (string.Equals(evt.Status, "Создано", StringComparison.OrdinalIgnoreCase))
            {
                return 0;
            }

            if (CompletedStatuses.Contains(evt.Status))
            {
                return FailedStatuses.Contains(evt.Status) || FailedResolutions.Contains(evt.Resolution)
                    ? 3
                    : 2;
            }

            if (FailedStatuses.Contains(evt.Status))
            {
                return 3;
            }

            return 1;
        }

        private static DateTime ParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return DateTime.MinValue;
            }

            string datePart = value.Split(' ')[0];
            return DateTime.TryParseExact(
                datePart,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime parsedDate)
                ? parsedDate
                : DateTime.MinValue;
        }

        public sealed class DistributionAnalysis
        {
            public int TotalEvents { get; }
            public double Average { get; }
            public double Variance { get; }
            public double StandardDeviation { get; }
            public double LowerLimit { get; }
            public double UpperLimit { get; }
            public int RowLength { get; }
            public bool VarianceIsAcceptable { get; }
            public bool ValuesAreInsideLimits { get; }
            public bool LowerLimitIsPositive { get; }
            public bool RowBalanceIsAcceptable { get; }
            public string Report { get; }

            public bool HasData => TotalEvents > 0;

            public DistributionAnalysis(
                int totalEvents,
                double average,
                double variance,
                double standardDeviation,
                double lowerLimit,
                double upperLimit,
                int rowLength,
                bool varianceIsAcceptable,
                bool valuesAreInsideLimits,
                bool lowerLimitIsPositive,
                bool rowBalanceIsAcceptable,
                string report)
            {
                TotalEvents = totalEvents;
                Average = average;
                Variance = variance;
                StandardDeviation = standardDeviation;
                LowerLimit = lowerLimit;
                UpperLimit = upperLimit;
                RowLength = rowLength;
                VarianceIsAcceptable = varianceIsAcceptable;
                ValuesAreInsideLimits = valuesAreInsideLimits;
                LowerLimitIsPositive = lowerLimitIsPositive;
                RowBalanceIsAcceptable = rowBalanceIsAcceptable;
                Report = report;
            }

            public static DistributionAnalysis Empty(string report)
            {
                return new DistributionAnalysis(0, 0, 0, 0, 0, 0, 0, false, false, false, false, report);
            }
        }

        public sealed class EventRecord
        {
            public int Id { get; }
            public string Area { get; }
            public string Type { get; }
            public string Status { get; }
            public string State { get; }
            public string Priority { get; }
            public string TicketNumber { get; }
            public string Name { get; }
            public DateTime CreationDate { get; }
            public string Creator { get; }
            public DateTime UpdatedDate { get; }
            public string Updator { get; }
            public string ParentTicket { get; }
            public string Assignee { get; }
            public string Owner { get; }
            public DateTime DueDate { get; }
            public string Rank { get; }
            public int Estimation { get; }
            public int SecondsSpent { get; }
            public string Workgroup { get; }
            public string Resolution { get; }

            public EventRecord(
                string id,
                string area,
                string type,
                string status,
                string state,
                string priority,
                string ticketNumber,
                string name,
                string creationDate,
                string creator,
                string updateDate,
                string updator,
                string parentTicket,
                string assignee,
                string owner,
                string dueDate,
                string rank,
                string estimation,
                string secondsSpent,
                string workgroup,
                string resolution)
            {
                Id = int.TryParse(id, out int parsedId) ? parsedId : 0;
                Area = area;
                Type = type;
                Status = status;
                State = state;
                Priority = priority;
                TicketNumber = ticketNumber;
                Name = name;
                CreationDate = ParseDate(creationDate);
                Creator = creator;
                UpdatedDate = ParseDate(updateDate);
                Updator = updator;
                ParentTicket = parentTicket;
                Assignee = assignee;
                Owner = owner;
                DueDate = ParseDate(dueDate);
                Rank = rank;
                Estimation = int.TryParse(estimation, out int parsedEstimation) ? parsedEstimation : 0;
                SecondsSpent = int.TryParse(secondsSpent, out int parsedSecondsSpent) ? parsedSecondsSpent : 0;
                Workgroup = workgroup;
                Resolution = resolution;
            }
        }

        public sealed class Sprint
        {
            public string Name { get; }
            public string Status { get; }
            public DateTime Start { get; }
            public DateTime End { get; }
            public List<int> EntityIds { get; }

            public Sprint(string name, string status, string start, string end, string entityIds)
            {
                Name = name;
                Status = status;
                Start = ParseDate(start);
                End = ParseDate(end);
                EntityIds = ParseEntityIds(entityIds);
            }

            private static List<int> ParseEntityIds(string value)
            {
                string normalized = value.Trim();
                if (normalized.Length < 2)
                {
                    return new List<int>();
                }

                return normalized
                    .Trim('{', '}')
                    .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(item => item.Trim())
                    .Where(item => int.TryParse(item, out _))
                    .Select(int.Parse)
                    .ToList();
            }
        }
    }
}
