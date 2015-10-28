using System;
using Cchbc.Objects;

namespace Cchbc.Search
{
	public sealed class FilterOption<T> : ViewObject where T : ViewObject
	{
		public string Name { get; }
		public Func<T, bool> IsMatch { get; }

		private bool _isSelected;
		public bool IsSelected
		{
			get { return _isSelected; }
			set { this.SetField(ref _isSelected, value); }
		}

		public FilterOption(string name, Func<T, bool> isMatch, bool isSelected = false)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (isMatch == null) throw new ArgumentNullException(nameof(isMatch));

			this.Name = name;
			this.IsMatch = isMatch;
			this.IsSelected = isSelected;
		}

		public void Flip()
		{
			this.IsSelected = !this.IsSelected;
		}
	}
}