using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Cchbc.Common;
using Cchbc.Data;
using Cchbc.Features.Admin.Providers;
using Cchbc.Features.Db.Objects;
using Cchbc.Objects;

namespace Cchbc.Features.Admin.FeatureDetailsModule
{
	public sealed class FeatureDetail
	{

	}

	public sealed class FeaturesHeader : ViewModel
	{
		public string Caption { get; }

		private FeatureSortOrder _sortOrder;
		public FeatureSortOrder SortOrder
		{
			get { return _sortOrder; }
			set { this.SetField(ref _sortOrder, value); }
		}

		public ICommand ChangeSortOrderCommand { get; }

		public FeaturesHeader(string caption, FeatureSortOrder sortOrder, ICommand changeSortOrderCommand)
		{
			if (caption == null) throw new ArgumentNullException(nameof(caption));
			if (sortOrder == null) throw new ArgumentNullException(nameof(sortOrder));

			this.Caption = caption;
			this._sortOrder = sortOrder;
			this.ChangeSortOrderCommand = changeSortOrderCommand;
		}
	}

	public sealed class FeatureSortOrder : ViewModel
	{
		public static readonly FeatureSortOrder Alphabetical = new FeatureSortOrder(@"Alphabetical");
		public static readonly FeatureSortOrder MostUsed = new FeatureSortOrder(@"Most Used");

		public string Name { get; }

		public FeatureSortOrder(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Name = name;
		}
	}

	public sealed class FeatureDetailsViewModel : ViewModel
	{
		private CommonDataProvider DataProvider { get; }
		private ITransactionContextCreator ContextCreator { get; }

		public string ContextsHeader { get; } = @"Screens";
		public FeaturesHeader FeaturesHeader { get; }
		

		public ObservableCollection<ContextViewModel> Contexts { get; } = new ObservableCollection<ContextViewModel>();
		public ObservableCollection<FeatureViewModel> Features { get; } = new ObservableCollection<FeatureViewModel>();

		private ContextViewModel _currentContext;
		public ContextViewModel CurrentContext
		{
			get { return _currentContext; }
			set
			{
				_currentContext = value;
				this.LoadCurrentFeatures();
			}
		}

		private FeatureViewModel _currentFeature;
		public FeatureViewModel CurrentFeature
		{
			get { return _currentFeature; }
			set
			{
				_currentFeature = value;
				this.LoadCurrentFeatures();
			}
		}

		public FeatureDetailsViewModel(CommonDataProvider dataProvider, ITransactionContextCreator contextCreator)
		{
			if (dataProvider == null) throw new ArgumentNullException(nameof(dataProvider));
			if (contextCreator == null) throw new ArgumentNullException(nameof(contextCreator));

			this.DataProvider = dataProvider;
			this.ContextCreator = contextCreator;

			this.FeaturesHeader = new FeaturesHeader(@"Features", FeatureSortOrder.Alphabetical, new RelayCommand(() =>
			{
				if (this.FeaturesHeader.SortOrder == FeatureSortOrder.Alphabetical)
				{
					this.FeaturesHeader.SortOrder = FeatureSortOrder.MostUsed;
				}
				else
				{
					if (this.FeaturesHeader.SortOrder == FeatureSortOrder.MostUsed)
					{
						this.FeaturesHeader.SortOrder = FeatureSortOrder.Alphabetical;
					}
				}

				this.LoadCurrentFeatures();
			}));

			this.Contexts.Add(new ContextViewModel(new DbContextRow(-1, @"All")));
			foreach (var context in dataProvider.Contexts.Values)
			{
				this.Contexts.Add(new ContextViewModel(context));
			}

			this.CurrentContext = this.Contexts[0];
		}

		private void LoadCurrentFeatures()
		{
			var contextId = this.CurrentContext.Context.Id;

			var relatedFeatures = new List<FeatureViewModel>();
			foreach (var feature in this.DataProvider.Features.Values)
			{
				if (feature.ContextId == contextId)
				{
					relatedFeatures.Add(new FeatureViewModel(feature));
				}
			}

			//_currentFeature

			if (this.FeaturesHeader.SortOrder == FeatureSortOrder.Alphabetical)
			{
				relatedFeatures.Sort((x, y) => string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase));
			}
			if (this.FeaturesHeader.SortOrder == FeatureSortOrder.MostUsed)
			{
				relatedFeatures.Sort((x, y) =>
				{
					var cmp = x.Count.CompareTo(y.Count);
					if (cmp == 0)
					{
						cmp = string.Compare(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
					}
					return cmp;
				});
			}

			this.Features.Clear();
			foreach (var viewModel in relatedFeatures)
			{
				this.Features.Add(viewModel);
			}
		}
	}

	public sealed class ContextViewModel : ViewModel
	{
		public DbContextRow Context { get; }

		public string Name { get; }

		public ContextViewModel(DbContextRow context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.Context = context;
			this.Name = context.Name;
		}
	}

	public sealed class FeatureViewModel : ViewModel
	{
		public DbFeatureRow Feature { get; }

		public string Name { get; }

		private int _count;
		public int Count
		{
			get { return _count; }
			set { this.SetField(ref _count, _count); }
		}

		public FeatureViewModel(DbFeatureRow feature)
		{
			if (feature == null) throw new ArgumentNullException(nameof(feature));

			this.Feature = feature;
			this.Name = feature.Name;
		}
	}
}