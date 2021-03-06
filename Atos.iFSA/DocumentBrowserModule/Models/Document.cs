using System;
using System.Collections.Generic;

namespace Atos.iFSA.DocumentBrowserModule.Models
{
	public sealed class Document
	{
		public long Id { get; }
		public string Name { get; }
		public HashSet<string> TradeChannels { get; } = new HashSet<string>();
		public HashSet<string> SubTradeChannels { get; } = new HashSet<string>();

		public Document(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}

		public bool HasMatch(string search)
		{
			if (search == null) throw new ArgumentNullException(nameof(search));

			if (search.Length > 0)
			{
				return this.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
			}

			return true;
		}

		public bool HasMatch(DocumentProperty property, IEnumerable<string> selectedCodes)
		{
			if (selectedCodes == null) throw new ArgumentNullException(nameof(selectedCodes));

			HashSet<string> codes;

			switch (property)
			{
				case DocumentProperty.TradeChannel:
					codes = this.TradeChannels;
					break;
				case DocumentProperty.SubTradeChannel:
					codes = this.SubTradeChannels;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return codes.Overlaps(selectedCodes);
		}
	}
}