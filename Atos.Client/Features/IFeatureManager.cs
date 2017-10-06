using System;

namespace Atos.Client.Features
{
	public interface IFeatureManager
	{
		void Save(Feature feature, string details = null);
		void Save(Feature feature, Exception exception);
	}
}