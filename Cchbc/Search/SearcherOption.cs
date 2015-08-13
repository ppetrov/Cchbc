using System;
using Cchbc.Objects;

namespace Cchbc.Search
{
	public class SearcherOption<T> : ViewObject where T : ViewObject
	{
		public string DisplayName { get; }
		public Func<T, bool> IsMatch { get; }

		private int _count;
		public int Count
		{
			get { return _count; }
			set { this.SetField(ref _count, value); }
		}

		public SearcherOption(string displayName, Func<T, bool> isMatch)
		{
			if (displayName == null) throw new ArgumentNullException(nameof(displayName));
			if (isMatch == null) throw new ArgumentNullException(nameof(isMatch));

			this.DisplayName = displayName;
			this.IsMatch = isMatch;
		}
	}
}