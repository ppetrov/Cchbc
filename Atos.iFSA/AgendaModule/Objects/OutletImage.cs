using System;

namespace iFSA.AgendaModule.Objects
{
	public sealed class OutletImage
	{
		public long Outlet { get; }
		public byte[] Data { get; }

		public OutletImage(long outlet, byte[] data)
		{
			if (data == null) throw new ArgumentNullException(nameof(data));

			this.Outlet = outlet;
			this.Data = data;
		}
	}
}