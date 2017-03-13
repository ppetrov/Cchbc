using System;

namespace Cchbc.Features.DashboardModule.ViewModels
{
	public sealed class DashboardVersionViewModel : ViewModel
	{
		public string Name { get; }
		public int Users { get; }
		public int Exceptions { get; }

		public DashboardVersionViewModel(DashboardVersion version)
		{
			if (version == null) throw new ArgumentNullException(nameof(version));

			this.Name = version.Version.Name;
			this.Users = version.Users;
			this.Exceptions = version.Exceptions;
		}
	}
}