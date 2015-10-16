using System;

namespace Cchbc.Data
{
	public sealed class ModifyDataAdapter
	{
		public ModifyDataQueryHelper ModifyDataQueryHelper { get; }

		public ModifyDataAdapter(ModifyDataQueryHelper modifyDataQueryHelper)
		{
			if (modifyDataQueryHelper == null) throw new ArgumentNullException(nameof(modifyDataQueryHelper));

			this.ModifyDataQueryHelper = modifyDataQueryHelper;
		}
	}
}