using System;
using Cchbc.Objects;

namespace Cchbc.Search
{
	public sealed class SearchOption<T> : ViewObject
	{
		public string DisplayName { get; }
		public Func<T, bool> IsMatch { get; }

		private int _count;
		public int Count
		{
			get { return _count; }
			set { this.SetField(ref _count, value); }
		}

		private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set { this.SetField(ref _isSelected, value); }
		}

		public SearchOption(string displayName, Func<T, bool> isMatch, bool isSelected = false)
		{
			if (displayName == null) throw new ArgumentNullException(nameof(displayName));
			if (isMatch == null) throw new ArgumentNullException(nameof(isMatch));

			this.DisplayName = displayName;
			this.IsMatch = isMatch;
			this.IsSelected = isSelected;
		}
	}
}