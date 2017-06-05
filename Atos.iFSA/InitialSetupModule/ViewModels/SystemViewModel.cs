using System;
using Atos.iFSA.InitialSetupModule.Objects;

namespace Atos.iFSA.InitialSetupModule.ViewModels
{
	public sealed class SystemViewModel
	{
		public AppSystem AppSystem { get; }
		public string Name => this.AppSystem.Name;
		public SystemSource Source => this.AppSystem.Source;

		public SystemViewModel(AppSystem appSystem)
		{
			if (appSystem == null) throw new ArgumentNullException(nameof(appSystem));

			this.AppSystem = appSystem;
		}
	}
}