using System;
using Cchbc.Features.Admin.FeatureExceptionModule.Objects;

namespace Cchbc.Features.Admin.FeatureExceptionModule.ViewModels
{
	public sealed class FeatureExceptionViewModel
	{
		public FeatureException FeatureException { get; }

		public string Context { get; }
		public string Feature { get; }
		public string User { get; }
		public string Message { get; }
		public string StackTrace { get; }
		public string CreateAt { get; }

		public FeatureExceptionViewModel(FeatureException featureException)
		{
			if (featureException == null) throw new ArgumentNullException(nameof(featureException));

			this.FeatureException = featureException;
			this.Context = featureException.Context;
			this.Feature = featureException.Feature;
			this.User = featureException.User;
			this.Message = featureException.Message;
			this.StackTrace = featureException.StackTrace;
			this.CreateAt = featureException.CreatedAt.ToString(@"s");
		}
	}
}