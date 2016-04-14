using System;
using Cchbc.Objects;

namespace Cchbc.Features.Db
{
	public sealed class DbFeatureEntry : IDbObject
	{
		public long Id { get; set; }
		public DbFeatureRow Feature { get; }
		public string Details { get; }
		public TimeSpan TimeSpent { get; }

		public DbFeatureEntry(long id, DbFeatureRow feature, string details, TimeSpan timeSpent)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (details == null) throw new ArgumentNullException(nameof(details));

			this.Id = id;
			this.Feature = feature;
			this.Details = details;
			this.TimeSpent = timeSpent;
		}
	}
}