using System;
using System.Diagnostics;
using Atos.Client.Features;

namespace Atos.iFSA.UI.LoginModule
{
	public sealed class DebugFeatureManager : IFeatureManager
	{
		public void Save(Feature feature, string details = null)
		{
			Debug.WriteLine(@"Save feature:" + feature.Context + ":" + feature.Name + details);
		}

		public void Save(Feature feature, Exception exception)
		{
			Debug.WriteLine(@"Save feature:" + feature.Context + ":" + feature.Name + Environment.NewLine + exception);
		}
	}
}