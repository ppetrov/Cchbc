using System;
using Cchbc.Features.ExceptionsModule.Rows;

namespace Cchbc.Features.ExceptionsModule
{
	public sealed class FeatureExceptionEntry
	{
		public long Id { get; }
		public FeatureRow Feature { get; }
		public ExceptionRow Exception { get; }
		public UserRow User { get; }
		public VersionRow Version { get; }
		public DateTime CreatedAt { get; }

		public string Message { get; }

		public FeatureExceptionEntry(long id, FeatureRow feature, ExceptionRow exception, UserRow user, VersionRow version, DateTime createdAt)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));
			if (exception == null) throw new ArgumentNullException(nameof(exception));
			if (user == null) throw new ArgumentNullException(nameof(user));
			if (version == null) throw new ArgumentNullException(nameof(version));

			this.Id = id;
			this.Feature = feature;
			this.Exception = exception;
			this.User = user;
			this.Version = version;
			this.CreatedAt = createdAt;
			this.Message = exception.Name;
		}
	}
}