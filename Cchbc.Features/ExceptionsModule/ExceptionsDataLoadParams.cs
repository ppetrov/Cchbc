using System;
using Cchbc.Data;
using Cchbc.Features.ExceptionsModule.Objects;

namespace Cchbc.Features.ExceptionsModule
{
	public sealed class ExceptionsDataLoadParams
	{
		public IDbContext Context { get; }
		public ExceptionsSettings Settings { get; set; }
		public FeatureVersion Version { get; set; }
		public TimePeriod TimePeriod { get; set; }

		public ExceptionsDataLoadParams(IDbContext context, ExceptionsSettings settings)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (settings == null) throw new ArgumentNullException(nameof(settings));

			this.Context = context;
			this.Settings = settings;
		}
	}
}