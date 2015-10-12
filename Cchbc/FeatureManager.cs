using System;
using System.Collections.Generic;

namespace Cchbc
{
	public sealed class FeatureManager
	{
		private Dictionary<string, Dictionary<string, long>> Contexts { get; } = new Dictionary<string, Dictionary<string, long>>();

		public ILogger Logger { get; }

		public FeatureManager(ILogger logger)
		{
			if (logger == null) throw new ArgumentNullException(nameof(logger));

			this.Logger = logger;
		}

		public void Use(string featureName)
		{
			if (featureName == null) throw new ArgumentNullException(nameof(featureName));

			Dictionary<string, long> features;

			// Search for features by context
			var context = this.Logger.Context;
			if (!this.Contexts.TryGetValue(context, out features))
			{
				features = new Dictionary<string, long>();
				this.Contexts.Add(context, features);
			}

			long count;
			if (features.TryGetValue(featureName, out count))
			{
				// Increment feature usage
				features[featureName] = count + 1;
			}
			else
			{
				// Init feature usage
				features.Add(featureName, 1);
			}
		}
	}
}