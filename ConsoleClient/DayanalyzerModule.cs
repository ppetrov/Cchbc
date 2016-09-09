using System;
using System.Collections.Generic;
using Cchbc.Data;

namespace ConsoleClient
{
	public sealed class FeatureUser
	{
		public long Id { get; }
		public string Name { get; set; }

		public FeatureUser(long id, string name)
		{
			this.Id = id;
			this.Name = name;
		}
	}

	public static class FeatureReportDataProvider
	{
		public static FeatureUser GetFeatureUser(ITransactionContext context, long userId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var sqlParams = new[]
			{
				new QueryParameter(@"@USER", userId),
			};

			var query = new Query<FeatureUser>(@"SELECT ID, NAME FROM FEATURE_USERS WHERE ID = @USER", FeatureUserCreator,
				sqlParams);

			var users = context.Execute(query);
			return users.Count > 0 ? users[0] : null;
		}

		public static Dictionary<long, FeatureUser> GetFeatureUsers(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var query = new Query<FeatureUser>(@"SELECT ID, NAME FROM FEATURE_USERS", FeatureUserCreator);

			var users = new Dictionary<long, FeatureUser>();
			context.Fill(users, r => r.Id, query);
			return users;
		}

		public static List<FeatureTimeEntry> GetFeatureTimeEntries(ITransactionContext context, DateTime date, long versionId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var value = date.Date;

			var sqlParams = new[]
			{
				new QueryParameter(@"@LOWER", value.AddDays(-1)),
				new QueryParameter(@"@UPPER", value.AddDays(1)),
				new QueryParameter(@"@VERSION", versionId),
			};

			var query =
				new Query<FeatureTimeEntry>(
					@"SELECT TIMESPENT, FEATURE_ID, USER_ID, VERSION_ID FROM FEATURE_ENTRIES WHERE @LOWER < CREATED_AT and CREATED_AT < @UPPER AND VERSION_ID = @VERSION",
					FeatureTimeEntryCreator, sqlParams);

			return context.Execute(query);
		}

		public static List<FeatureTimeEntry> GetFeatureTimeEntries(ITransactionContext context, DateTime date, long versionId,
			long userId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			var value = date.Date;

			var sqlParams = new[]
			{
				new QueryParameter(@"@LOWER", value.AddDays(-1)),
				new QueryParameter(@"@UPPER", value.AddDays(1)),
				new QueryParameter(@"@VERSION", versionId),
				new QueryParameter(@"@USER", userId),
			};

			var query =
				new Query<FeatureTimeEntry>(
					@"SELECT TIMESPENT, FEATURE_ID, USER_ID, VERSION_ID FROM FEATURE_ENTRIES WHERE @LOWER < CREATED_AT and CREATED_AT < @UPPER AND VERSION_ID = @VERSION AND USER_ID = @USER",
					FeatureTimeEntryCreator, sqlParams);

			return context.Execute(query);
		}

		private static FeatureTimeEntry FeatureTimeEntryCreator(IFieldDataReader r)
		{
			return new FeatureTimeEntry(Convert.ToDouble(r.GetDecimal(0)), r.GetInt64(1), r.GetInt64(2), r.GetInt64(3));
		}

		public static long GetLatestVersionId(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			// TODO : Find the latest version
			var query = @"SELECT ID, NAME FROM FEATURE_VERSIONS";

			var versions =
				context.Execute(new Query<FeatureVersion>(query, r => new FeatureVersion(r.GetInt64(0), r.GetString(1))));

			var tmp = new Version[versions.Count];
			for (var i = 0; i < versions.Count; i++)
			{
				var version = versions[i];
				var numbers = version.Name.Split('.');

				var index = 0;
				var major = GetValueAt(numbers, index++);
				var minor = GetValueAt(numbers, index++);
				var revision = GetValueAt(numbers, index++);
				var build = GetValueAt(numbers, index);

				tmp[i] = new Version(version.Id, major, minor, revision, build);
			}

			Array.Sort(tmp, (x, y) =>
			{
				var cmp = x.Major.CompareTo(y.Major);
				if (cmp == 0)
				{
					cmp = x.Minor.CompareTo(y.Minor);
					if (cmp == 0)
					{
						cmp = x.Revision.CompareTo(y.Revision);
						if (cmp == 0)
						{
							cmp = x.Build.CompareTo(y.Build);
						}
					}
				}
				return cmp;
			});

			return tmp[tmp.Length - 1].Id;
		}

		private static int GetValueAt(string[] numbers, int index)
		{
			if (index < numbers.Length)
			{
				int value;
				if (int.TryParse(numbers[index], out value))
				{
					return value;
				}
			}
			return 0;
		}

		private static FeatureUser FeatureUserCreator(IFieldDataReader r)
		{
			return new FeatureUser(r.GetInt64(0), r.GetString(1));
		}

		private sealed class Version
		{
			public readonly long Id;
			public readonly int Major;
			public readonly int Minor;
			public readonly int Revision;
			public readonly int Build;

			public Version(long id, int major, int minor, int revision, int build)
			{
				this.Id = id;
				this.Major = major;
				this.Minor = minor;
				this.Revision = revision;
				this.Build = build;
			}
		}
	}

	public sealed class FeatureTimeEntry
	{
		public readonly double TimeSpent;
		public readonly long FeatureId;
		public readonly long UserId;
		public readonly long VersionId;

		public FeatureTimeEntry(double timeSpent, long featureId, long userId, long versionId)
		{
			this.TimeSpent = timeSpent;
			this.FeatureId = featureId;
			this.UserId = userId;
			this.VersionId = versionId;
		}
	}















	public sealed class FeatureReportSettings
	{
		public int SlowestFeatures { get; set; }
		public int MostUsedFeatures { get; set; }
		public int LeastUsedFeatures { get; set; }
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
		public FeatureTimes[] SlowestFeatures { get; }
		public FeatureTimes[] MostUsedFeatures { get; }
		public FeatureTimes[] LeastUsedFeatures { get; }

		public FeatureReport(FeatureUser user, FeatureTimes[] slowestFeatures, FeatureTimes[] mostUsedFeatures,
			FeatureTimes[] leastUsedFeatures)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (slowestFeatures == null) throw new ArgumentNullException(nameof(slowestFeatures));
			if (mostUsedFeatures == null) throw new ArgumentNullException(nameof(mostUsedFeatures));
			if (leastUsedFeatures == null) throw new ArgumentNullException(nameof(leastUsedFeatures));

			this.User = user;
			this.SlowestFeatures = slowestFeatures;
			this.MostUsedFeatures = mostUsedFeatures;
			this.LeastUsedFeatures = leastUsedFeatures;
		}
	}

	public static class FeatureAnalyzer
	{
		public static List<FeatureReport> GetFeatureReport(ITransactionContext context, FeatureReportSettings settings, DateTime date)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (settings == null) throw new ArgumentNullException(nameof(settings));

			var versionId = FeatureReportDataProvider.GetLatestVersionId(context);

			return GetFeatureReport(context, settings, date, versionId);
		}

		public static List<FeatureReport> GetFeatureReport(ITransactionContext context, FeatureReportSettings settings, DateTime date, long versionId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (settings == null) throw new ArgumentNullException(nameof(settings));

			var users = FeatureReportDataProvider.GetFeatureUsers(context);
			var entries = FeatureReportDataProvider.GetFeatureTimeEntries(context, date, versionId);

			entries.Sort((x, y) =>
			{
				var cmp = x.UserId.CompareTo(y.UserId);
				if (cmp == 0)
				{
					cmp = x.FeatureId.CompareTo(y.FeatureId);
				}
				return cmp;
			});

			var reports = new List<FeatureReport>();

			var i = 0;
			while (i < entries.Count)
			{
				var userId = entries[i].UserId;

				var count = i + 1;
				while (count < entries.Count && userId == entries[count].UserId)
				{
					count++;
				}

				var report = ExtractFeatureReport(users[userId], settings, entries, i, count);
				reports.Add(report);

				i += count;
			}

			return reports;
		}

		public static FeatureReport GetFeatureReport(ITransactionContext context, FeatureReportSettings settings, DateTime date, long versionId, long userId)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (settings == null) throw new ArgumentNullException(nameof(settings));

			var featureUser = FeatureReportDataProvider.GetFeatureUser(context, userId);
			var entries = FeatureReportDataProvider.GetFeatureTimeEntries(context, date, versionId, userId);

			// Sort features to be able to "group" them by FeatureId
			entries.Sort((x, y) => x.FeatureId.CompareTo(y.FeatureId));

			return ExtractFeatureReport(featureUser, settings, entries, 0, entries.Count);
		}

		private static FeatureReport ExtractFeatureReport(FeatureUser featureUser, FeatureReportSettings settings, List<FeatureTimeEntry> entries, int startIndex, int endIndex)
		{
			var featureTimes = new List<FeatureTimes>(16);

			while (startIndex < endIndex)
			{
				var x = entries[startIndex];

				var timeSpent = x.TimeSpent;
				var min = timeSpent;
				var max = timeSpent;
				var totalTimeSpent = timeSpent;
				var count = 1;

				var current = x.FeatureId;

				for (var j = startIndex + 1; j < endIndex; j++)
				{
					var y = entries[j];
					if (y.FeatureId == current)
					{
						var value = y.TimeSpent;

						min = Math.Min(min, value);
						max = Math.Max(max, value);
						totalTimeSpent += value;
						count++;

						continue;
					}
					break;
				}

				featureTimes.Add(new FeatureTimes(x.FeatureId, count, totalTimeSpent / count, min, max));

				startIndex += count;
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

			return new FeatureReport(featureUser, slowest, mostUsed, leastUsed);
		}
	}

}