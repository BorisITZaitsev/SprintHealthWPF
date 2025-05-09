using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Statistic_functions
{

    public class DataTools
    {
        public static Dictionary<Sprint, List<Event>> completeDatabase;
        public static List<Sprint> sprints;
        public static List<Event> events;
        public static bool parameter1;
        public static bool parameter2;
        public static bool parameter3;
        public static bool parameter4;
        public static double standardDeviationOfEvents;
        public static double avarageOfEvents;
        public static string reportation;

        public static void CreateCompleteDatabase()
        {
            completeDatabase = new Dictionary<Sprint, List<Event>>();

            foreach (var sprint in sprints)
            {
                completeDatabase[sprint] = new List<Event>(events.Distinct().ToList());
            }
        }

        public static string AnalyzeDistribution(Dictionary<DateTime, int[]> RWFstatistics)
        {

            List<int> elementsList = new List<int>();
            foreach (DateTime date in RWFstatistics.Keys)
            {
                elementsList.Add(RWFstatistics[date].Sum());
            }
            int[] elements = elementsList.ToArray();

            int sumOfElements = elements.Sum(); ///Количество всех событий
            double avarage = sumOfElements / elementsList.Count; ///Среднее выборочное
            double sumOfSquaresOfDifferences = 0; ///Сумма квадратов разностей
            foreach (int element in elements) sumOfSquaresOfDifferences += (element - avarage) * (element - avarage);
            double variance = sumOfSquaresOfDifferences / elements.Length; ///Дисперсия
            double standardDeviation = Math.Sqrt(variance);///Станд. отклонение
            double borderA = avarage - standardDeviation * 2;///Нижняя граница
            double borderB = avarage + standardDeviation * 2;///Верхняя граница
            List<int> underRowsSums = new List<int>();///Суммы событий в подрядах
            int rowLength = (int) Math.Round(elements.Length * 0.2727, 0);///Длина подряда
            for (int i = 0; i < elements.Length - rowLength; i++) underRowsSums.Add(elementsList.GetRange(i, rowLength).Sum());
            int[] underRowsSumsInt = underRowsSums.ToArray();

            string report = $"The number of all events: {sumOfElements}" +
                $"\nthe average sample: {avarage}" +
                $"\nSum of squares of differences: {sumOfSquaresOfDifferences}" +
                $"\nStandard deviation: {standardDeviation}" +
                $"\nThe lower limit: {borderA}" +
                $"\nThe upper limit: {borderB}" +
                $"\nRow length: {rowLength}";

            standardDeviationOfEvents = standardDeviation;
            avarageOfEvents = avarage;

            parameter1 = (variance <= sumOfElements);
            parameter2 = (Array.FindAll(elements, x => (borderA <= x) && (borderB >= x)).Length == elements.Length);
            parameter3 = (borderA >= 0);
            parameter4 = (Array.FindAll(underRowsSumsInt, x => (x > 0.29 * sumOfElements)).Length == 0);

            reportation = report;
            return report;


            
        }

        public static void ConvertToEvents(List<string> data)
        {
            events = data.Select(line =>
            {
                var row = line.Split(';');
                return new Event(row[0], row[1], row[2], row[3], row[4],
                    row[5], row[6], row[7], row[8], row[9], row[10], row[11], row[12],
                    row[13], row[14], row[15], row[16], row[17], row[18], row[19], row[20]);
            }).ToList();
        }

        public static void ConvertToSprints(List<string> data)
        {
            sprints = data.Select(line =>
            {
                var row = line.Split(';');
                return new Sprint(row[0], row[1], row[2], row[3], row[4]);
            }).ToList();
        }

        public static List<string> ExtractData(string filepath)
        {
            return File.ReadAllLines(filepath, Encoding.UTF8).ToList();
        }

        public static int[] Backlog(List<Event> events, DateTime sprintStart, DateTime sprintFinish)
        {
            var backlogCounts = new int[3];
            foreach (var e in events)
            {
                if (e.type != "Дефект")
                {
                    if (e.creationDate > sprintStart.AddDays(2)) backlogCounts[0] += e.GetEstimation();
                    else backlogCounts[1] += e.GetEstimation();
                }
            }
            backlogCounts[2] = backlogCounts[1] > 0 ? (backlogCounts[0] * 100 / backlogCounts[1]) : 0;
            return backlogCounts;
        }

        public static (Dictionary<DateTime, int[]>, int[]) ReadyWorkFinished(List<Event> events, DateTime sprintStart, DateTime sprintFinish)
        {
            var result = new Dictionary<DateTime, int[]>();
            int[] totals = new int[4];

            foreach (var e in events.Where(e => e.creationDate > sprintStart && e.creationDate < sprintFinish && e.status != "Дефект"))
            {
                if (!result.ContainsKey(e.creationDate))
                {
                    result[e.creationDate] = new int[4];
                }

                switch (e.status)
                {
                    case "Создано":
                        result[e.creationDate][0]++;
                        totals[0]++;
                        break;
                    case "Закрыто":
                    case "Выполнено":
                        if ("ОтклоненоОтменено инициаторомДубликат".Contains(e.resolution) || e.status == "Отклонен исполнителем")
                        {
                            result[e.creationDate][3]++;
                            totals[3]++;
                        }
                        else
                        {
                            result[e.creationDate][2]++;
                            totals[2]++;
                        }
                        break;
                    default:
                        result[e.creationDate][1]++;
                        totals[1]++;
                        break;
                }
            }
            return (result, totals);
        }

        public static Dictionary<DateTime, int[]> GetUniformityOfTransitions(List<Event> events)
        {
            var result = new Dictionary<DateTime, int[]>();
            int[] sums = new int[3];

            foreach (var e in events)
            {
                if (!result.ContainsKey(e.creationDate))
                {
                    result[e.creationDate] = new int[3];
                }

                if (e.status == "Создано")
                {
                    sums[0]++;
                    result[e.creationDate][0]++;
                }
                else if (e.status != "Закрыто" && e.status != "Выполнено")
                {
                    sums[1]++;
                    result[e.creationDate][1]++;
                }
                else if (e.status == "Выполнено")
                {
                    sums[2]++;
                    result[e.creationDate][2]++;
                }
            }
            return result;
        }

        public static Dictionary<string, int> ResolutionBalance(List<Event> events)
        {
            return events.Where(e => !string.IsNullOrEmpty(e.resolution))
                         .GroupBy(e => e.resolution)
                         .ToDictionary(g => g.Key, g => g.Count());
        }

        public class Event
        {
            public int id { get; }
            public string type { get; }
            public string status { get; }
            public DateTime creationDate { get; }
            private string area;
            private string state;
            private string priority;
            private string ticketNumber;
            private string name;
            private string creator;
            private DateTime updatedDate;
            private string updator;
            private string parentTicket;
            private string assignee;
            private string owner;
            private DateTime dueDate;
            private string rank;
            private int estimation;
            private int secondsSpent;
            public string resolution { get; }

            public Event(string id, string area, string type, string status, string state,
                string priority, string ticketNumber, string name, string creationDate, string creator,
                string updateDate, string updator, string parentTicket, string assignee, string owner,
                string dueDate, string rank, string estimation, string secondsSpent, string workgroup, string resolution)
            {
                this.id = Convert.ToInt32(id);
                this.area = area;
                this.type = type;
                this.status = status;
                this.state = state;
                this.priority = priority;
                this.ticketNumber = ticketNumber;
                this.name = name;
                this.creationDate = ParseDate(creationDate);
                this.creator = creator;
                this.updatedDate = ParseDate(updateDate);
                this.updator = updator;
                this.parentTicket = parentTicket;
                this.assignee = assignee;
                this.owner = owner;
                this.dueDate = ParseDate(dueDate);
                this.rank = rank;
                this.estimation = int.TryParse(estimation, out var est) ? est : 0;
                this.secondsSpent = int.TryParse(secondsSpent, out var seconds) ? seconds : 0;
                this.resolution = resolution;
            }

            private DateTime ParseDate(string dateStr)
            {
                if (string.IsNullOrWhiteSpace(dateStr) || !dateStr.Contains("-")) return DateTime.MinValue;

                var parts = dateStr.Split(' ')[0].Split('-');

                return new DateTime(Convert.ToInt32(parts[0]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[2]));
            }

            public string InformationOutput()
            {
                var stringBuilder = new StringBuilder()
                    .Append(id).Append("\t")
                    .Append(area).Append("\t")
                    .Append(type).Append("\t")
                    .Append(status).Append("\t")
                    .Append(state).Append("\t")
                    .Append(priority).Append("\t")
                    .Append(ticketNumber).Append("\t")
                    .Append(name).Append("\t")
                    .Append(creationDate).Append("\t")
                    .Append(creator).Append("\t")
                    .Append(updatedDate).Append("\t")
                    .Append(updator).Append("\t")
                    .Append(parentTicket).Append("\t")
                    .Append(assignee).Append("\t")
                    .Append(owner).Append("\t")
                    .Append(dueDate).Append("\t")
                    .Append(rank);
                return stringBuilder.ToString();
            }

            public int GetEstimation()
            {
                return this.estimation;
            }
        }

        public class Sprint
        {
            public string name { get; }
            public string status { get; }
            public DateTime start { get; }
            public DateTime end { get; }
            public List<int> entityIds { get; }

            public Sprint(string name, string status, string start, string end, string entityIds)
            {
                this.name = name;
                this.status = status;
                this.start = ParseDate(start);
                this.end = ParseDate(end);
                string[] ids = entityIds.Substring(1, entityIds.Length - 2).Split(',');
                List<int> intIds = new List<int>();
                foreach (string id in ids)
                {
                    intIds.Add(Convert.ToInt32(id));
                }
                this.entityIds = intIds;
            }

            private DateTime ParseDate(string dateStr)
            {
                if (string.IsNullOrWhiteSpace(dateStr) || !dateStr.Contains("-")) return DateTime.MinValue;

                var parts = dateStr.Split(' ')[0].Split('-');

                return new DateTime(Convert.ToInt32(parts[0]), Convert.ToInt32(parts[1]), Convert.ToInt32(parts[2]));
            }

            public string InformationOutput()
            {
                var stringBuilder = new StringBuilder()
                    .Append(name).Append("\t")
                    .Append(status).Append("\t")
                    .Append(start).Append("\t")
                    .Append(end).Append("\t");

                stringBuilder.Append(string.Join("\t", entityIds));
                return stringBuilder.ToString();
            }
        }
    }
}