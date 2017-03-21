using System;
using System.Diagnostics;
using Cchbc.Features;

namespace UIDemo
{
	public sealed class DebugFeatureManager : IFeatureManager
	{
		public void Load()
		{
			Debug.WriteLine(@"Load feature manager");
		}

		public void Save(Feature feature, string details = null)
		{
			Debug.WriteLine(@"Save feature manager");
		}

		public void Save(Feature feature, Exception exception)
		{
			Debug.WriteLine(@"Save feature manager - exception");
		}
	}
}