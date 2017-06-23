using System;
using iFSA.AgendaModule.Objects;

namespace Atos.iFSA.AgendaModule.Objects
{
	public sealed class OutletImageEventArgs : EventArgs
	{
		public OutletImage OutletImage { get; }

		public OutletImageEventArgs(OutletImage outletImage)
		{
			if (outletImage == null) throw new ArgumentNullException(nameof(outletImage));

			this.OutletImage = outletImage;
		}
	}
}