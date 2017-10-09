using System;
using Atos.Client;
using Atos.iFSA.Objects;

namespace Atos.iFSA.AddActivityModule
{
	public sealed class OutletViewModel : ViewModel
	{
		public Outlet Outlet { get; }
		public bool IsSuppressed => this.Outlet.IsSuppressed;
		public string Number { get; }
		public string Name { get; }

		public OutletViewModel(Outlet outlet)
		{
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));

			this.Outlet = outlet;
			this.Number = outlet.Id.ToString();
			this.Name = outlet.Name;
		}
	}
}