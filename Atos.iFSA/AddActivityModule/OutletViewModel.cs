using Atos.Client;
using Atos.iFSA.Objects;

namespace Atos.iFSA.AddActivityModule
{
	public sealed class OutletViewModel : ViewModel<Outlet>
	{
		public string Number { get; }
		public string Name { get; }

		public OutletViewModel(Outlet model) : base(model)
		{
			this.Number = model.Id.ToString();
			this.Name = model.Name;
		}
	}
}