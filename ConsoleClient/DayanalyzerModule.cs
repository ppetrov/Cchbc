using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;

namespace ConsoleClient
{
    public sealed class FeatureUser
    {
        public string Name { get; }

        public FeatureUser(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            this.Name = name;
        }
    }

    public sealed class DayAnalyzeSettings
    {
        public int SlowestFeatures { get; set; }
        public int MostUsedFeatures { get; set; }
        public int LeastUsedFeatures { get; set; }
    }

    public sealed class FeatureTimeEntry
    {
        public readonly long FeatureId;
        public readonly double TimeSpent;

        public FeatureTimeEntry(long featureId, double timeSpent)
        {
            this.FeatureId = featureId;
            this.TimeSpent = timeSpent;
        }
    }

    public static class DayAnalyzerDataProvider
    {
        public static Task<List<FeatureTimeEntry>> GetFeatureTimeEntriesAsync(ITransactionContext context, DateTime date)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var value = date.Date;

            var sqlParams = new[]
            {
                new QueryParameter(@"@LOWER", value.AddDays(-1)),
                new QueryParameter(@"@UPPER", value.AddDays(1)),
            };

            var query = new Query<FeatureTimeEntry>(@"SELECT TIMESPENT, FEATURE_ID FROM FEATURE_ENTRIES WHERE @LOWER < CREATED_AT and CREATED_AT < @UPPER", r => new FeatureTimeEntry(r.GetInt64(1), Convert.ToDouble(r.GetDecimal(0))), sqlParams);

            return Task.FromResult(context.Execute(query));
        }
    }

    public sealed class FeatureTimes
    {
        public readonly long Id;
        public readonly int Count;
        public readonly double Avg;
        public readonly double Min;
        public readonly double Max;

        public FeatureTimes(long id, int count, double avg, double min, double max)
        {
            this.Id = id;
            this.Count = count;
            this.Avg = avg;
            this.Min = min;
            this.Max = max;
        }
    }

    public sealed class FeatureReport
    {
        public FeatureUser User { get; }
        public DateTime Date { get; }
        public FeatureTimes[] SlowestFeatures { get; }
        public FeatureTimes[] MostUsedFeatures { get; }
        public FeatureTimes[] LeastUsedFeatures { get; }

        public FeatureReport(FeatureUser user, DateTime date, FeatureTimes[] slowestFeatures, FeatureTimes[] mostUsedFeatures, FeatureTimes[] leastUsedFeatures)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (slowestFeatures == null) throw new ArgumentNullException(nameof(slowestFeatures));
            if (mostUsedFeatures == null) throw new ArgumentNullException(nameof(mostUsedFeatures));
            if (leastUsedFeatures == null) throw new ArgumentNullException(nameof(leastUsedFeatures));

            this.User = user;
            this.Date = date;
            this.SlowestFeatures = slowestFeatures;
            this.MostUsedFeatures = mostUsedFeatures;
            this.LeastUsedFeatures = leastUsedFeatures;
        }
    }

    public static class FeatureAnalyzer
    {
        // TODO : !!! Generate a report for the latest version for all users

        public static async Task<FeatureReport> GetFeatureReportAsync(ITransactionContext context, DateTime date, DayAnalyzeSettings settings)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));
            if (settings == null) throw new ArgumentNullException(nameof(settings));

            var entries = await DayAnalyzerDataProvider.GetFeatureTimeEntriesAsync(context, date);

            entries.Sort((x, y) => x.FeatureId.CompareTo(y.FeatureId));

            var featureUser = new FeatureUser(@"N/A");
            var featureTimes = new List<FeatureTimes>();

            var index = 0;
            while (index < entries.Count)
            {
                var x = entries[index];

                var timeSpent = x.TimeSpent;
                var min = timeSpent;
                var max = timeSpent;
                var total = timeSpent;
                var count = 1;

                var current = x.FeatureId;
                for (var j = index + 1; j < entries.Count; j++)
                {
                    var y = entries[j];
                    if (y.FeatureId == current)
                    {
                        var value = y.TimeSpent;

                        min = Math.Min(min, value);
                        max = Math.Max(max, value);
                        total += value;
                        count++;

                        continue;
                    }
                    break;
                }

                featureTimes.Add(new FeatureTimes(x.FeatureId, count, total / count, min, max));

                index += count;
            }

            // Sort descending by Avg
            featureTimes.Sort((x, y) => y.Avg.CompareTo(x.Avg));

            var slowest = new FeatureTimes[Math.Min(featureTimes.Count, settings.SlowestFeatures)];
            for (var i = 0; i < slowest.Length; i++)
            {
                slowest[i] = featureTimes[i];
            }

            featureTimes.Sort((x, y) => y.Count.CompareTo(x.Count));
            var mostUsed = new FeatureTimes[Math.Min(featureTimes.Count, settings.MostUsedFeatures)];
            for (var i = 0; i < mostUsed.Length; i++)
            {
                mostUsed[i] = featureTimes[i];
            }

            var leastUsed = new FeatureTimes[Math.Min(featureTimes.Count, settings.LeastUsedFeatures)];
            for (var i = 0; i < leastUsed.Length; i++)
            {
                leastUsed[i] = featureTimes[featureTimes.Count - 1 - i];
            }


            return new FeatureReport(featureUser, date, slowest, mostUsed, leastUsed);
        }
    }
}