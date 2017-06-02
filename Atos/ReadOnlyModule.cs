using System;
using System.Collections.Generic;

namespace Atos
{
	//public sealed class ReadOnlyModule<T, TViewModel> where TViewModel : ViewModel<T>
	//{
	//	private TViewModel[] ViewModels { get; set; }

	//	public Sorter<TViewModel> Sorter { get; }
	//	public Searcher<TViewModel> Searcher { get; }
	//	public FilterOption<TViewModel>[] FilterOptions { get; }

	//	public ReadOnlyModule(Sorter<TViewModel> sorter, Searcher<TViewModel> searcher, FilterOption<TViewModel>[] filterOptions = null)
	//	{
	//		if (sorter == null) throw new ArgumentNullException(nameof(sorter));
	//		if (searcher == null) throw new ArgumentNullException(nameof(searcher));

	//		this.Sorter = sorter;
	//		this.Searcher = searcher;
	//		this.FilterOptions = filterOptions;
	//	}

	//	public void SetupViewModels(TViewModel[] viewModels)
	//	{
	//		if (viewModels == null) throw new ArgumentNullException(nameof(viewModels));

	//		this.ViewModels = viewModels;
	//		this.Sorter.Sort(this.ViewModels, this.Sorter.CurrentOption);
	//	}

	//	public IEnumerable<TViewModel> Sort(ICollection<TViewModel> currentViewModels, SortOption<TViewModel> sortOption)
	//	{
	//		if (currentViewModels == null) throw new ArgumentNullException(nameof(currentViewModels));
	//		if (sortOption == null) throw new ArgumentNullException(nameof(sortOption));

	//		var flag = sortOption.Ascending ?? true;

	//		// Sort view items
	//		this.Sorter.Sort(this.ViewModels, sortOption);

	//		// Sort current view items
	//		var copy = new TViewModel[currentViewModels.Count];
	//		currentViewModels.CopyTo(copy, 0);
	//		this.Sorter.Sort(copy, sortOption);
	//		this.Sorter.SetupFlag(sortOption, flag);

	//		// Return current view items sorted
	//		foreach (var viewModel in copy)
	//		{
	//			yield return viewModel;
	//		}
	//	}

	//	public IEnumerable<TViewModel> Search(string textSearch, SearchOption<TViewModel> searchOption)
	//	{
	//		if (textSearch == null) throw new ArgumentNullException(nameof(textSearch));

	//		return this.Searcher.Search(GetFilteredViewModels(this.ViewModels), textSearch, searchOption);
	//	}

	//	private ICollection<TViewModel> GetFilteredViewModels(ICollection<TViewModel> viewModels)
	//	{
	//		if (this.FilterOptions != null && this.FilterOptions.Length > 0)
	//		{
	//			viewModels = new List<TViewModel>();

	//			foreach (var item in this.ViewModels)
	//			{
	//				var include = true;

	//				foreach (var filter in this.FilterOptions)
	//				{
	//					if (filter.IsSelected)
	//					{
	//						include &= filter.IsMatch(item);
	//						if (!include)
	//						{
	//							break;
	//						}
	//					}
	//				}

	//				if (include)
	//				{
	//					viewModels.Add(item);
	//				}
	//			}
	//		}

	//		return viewModels;
	//	}
	//}
}