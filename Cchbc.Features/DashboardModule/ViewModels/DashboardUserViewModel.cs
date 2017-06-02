using System;

namespace Atos.Features.DashboardModule.ViewModels
{
	public sealed class DashboardUserViewModel : ViewModel
	{
		public string Name { get; }
		public string Version { get; }
		public string ReplicatedAt { get; }

		public DashboardUserViewModel(DashboardUser user)
		{
			if (user == null) throw new ArgumentNullException(nameof(user));

			this.Name = user.Name;
			this.Version = user.Version;
			this.ReplicatedAt = user.ReplicatedAt.ToString(@"T");
		}
	}
}