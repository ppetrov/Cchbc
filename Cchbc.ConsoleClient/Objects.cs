using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Features;
using Cchbc.Objects;
using Cchbc.Search;
using Cchbc.Sort;

namespace Cchbc.ConsoleClient
{


















	public sealed class Outlet
	{
		public long Id { get; }
		public string Name { get; }

		public Outlet(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class OutletAdapter : IReadOnlyAdapter<Outlet>
	{
		private QueryExecutor QueryExecutor { get; }

		public OutletAdapter(QueryExecutor queryExecutor)
		{
			if (queryExecutor == null) throw new ArgumentNullException(nameof(queryExecutor));

			this.QueryExecutor = queryExecutor;
		}

		public void Fill(Dictionary<long, Outlet> items, Func<Outlet, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			var query = @"SELECT Id, Name FROM Outlets";

			this.QueryExecutor.Fill(new Query<Outlet>(query, this.OutletCreator), items, selector);
		}

		private Outlet OutletCreator(IFieldDataReader r)
		{
			var id = r.GetInt64(0);
			var name = string.Empty;
			if (!r.IsDbNull(1))
			{
				name = r.GetString(1);
			}

			return new Outlet(id, name);
		}
	}

	public sealed class Visit : IDbObject
	{
		public long Id { get; set; }
		public Outlet Outlet { get; set; }
		public DateTime Date { get; set; }
		public List<Activity> Activities { get; set; }

		public Visit(long id, Outlet outlet, DateTime date, List<Activity> activities)
		{
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));
			if (activities == null) throw new ArgumentNullException(nameof(activities));

			this.Id = id;
			this.Outlet = outlet;
			this.Date = date;
			this.Activities = activities;
		}
	}

	public sealed class VisitAdapter : IModifiableAdapter<Visit>
	{
		private QueryExecutor QueryExecutor { get; }
		private Dictionary<long, Outlet> Outlets { get; }
		private Dictionary<long, ActivityType> ActivityTypes { get; }

		public VisitAdapter(QueryExecutor queryExecutor, Dictionary<long, Outlet> outlets, Dictionary<long, ActivityType> activityTypes)
		{
			if (queryExecutor == null) throw new ArgumentNullException(nameof(queryExecutor));
			if (outlets == null) throw new ArgumentNullException(nameof(outlets));
			if (activityTypes == null) throw new ArgumentNullException(nameof(activityTypes));

			this.QueryExecutor = queryExecutor;
			this.Outlets = outlets;
			this.ActivityTypes = activityTypes;
		}

		public List<Visit> GetAll()
		{
			var query = @"SELECT v.Id, v.OutletId, v.Date, a.Id, a.Date, a.ActivityTypeId FROM Visits v INNER JOIN Activities a ON v.Id = a.VisitId";

			var visits = new Dictionary<long, Visit>();
			this.QueryExecutor.Fill(query, visits, this.VisitCreator);
			return visits.Values.ToList();
		}

		public Task InsertAsync(Visit item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pOutletId", item.Outlet.Id),
			new QueryParameter(@"@pDate", item.Date),
		};

			var query = @"INSERT INTO Visits (OutletId, Date) VALUES (@pOutletId, @pDate)";

			this.QueryExecutor.Execute(query, sqlParams);

			item.Id = this.QueryExecutor.GetNewId();

			return Task.FromResult(true);
		}

		public Task UpdateAsync(Visit item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pId", item.Id),
			new QueryParameter(@"@pOutletId", item.Outlet.Id),
			new QueryParameter(@"@pDate", item.Date),
		};

			var query = @"UPDATE Visits SET OutletId = @pOutletId, Date = @pDate WHERE Id = @pId";

			this.QueryExecutor.Execute(query, sqlParams);

			return Task.FromResult(true);
		}

		public Task DeleteAsync(Visit item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pId", item.Id),
		};

			var query = @"DELETE FROM Visits WHERE Id = @pId";

			this.QueryExecutor.Execute(query, sqlParams);
			return Task.FromResult(true);
		}

		private void VisitCreator(IFieldDataReader r, Dictionary<long, Visit> visits)
		{
			var id = r.GetInt64(0);

			Visit visit;
			if (!visits.TryGetValue(id, out visit))
			{
				var outlet = default(Outlet);
				if (!r.IsDbNull(1))
				{
					outlet = this.Outlets[r.GetInt64(1)];
				}
				var date = DateTime.MinValue;
				if (!r.IsDbNull(2))
				{
					date = r.GetDateTime(2);
				}
				visit = new Visit(id, outlet, date, new List<Activity>());
				visits.Add(id, visit);
			}
			var activityId = 0L;
			if (!r.IsDbNull(3))
			{
				activityId = r.GetInt64(3);
			}
			var activityDate = DateTime.MinValue;
			if (!r.IsDbNull(4))
			{
				activityDate = r.GetDateTime(4);
			}
			var activityActivityType = default(ActivityType);
			if (!r.IsDbNull(5))
			{
				activityActivityType = this.ActivityTypes[r.GetInt64(5)];
			}

			var activity = new Activity(activityId, activityDate, activityActivityType, visit);
			visit.Activities.Add(activity);
		}
	}

	public sealed class ActivityType
	{
		public long Id { get; }
		public string Name { get; }

		public ActivityType(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class ActivityTypeAdapter : IReadOnlyAdapter<ActivityType>
	{
		private QueryExecutor QueryExecutor { get; }

		public ActivityTypeAdapter(QueryExecutor queryExecutor)
		{
			if (queryExecutor == null) throw new ArgumentNullException(nameof(queryExecutor));

			this.QueryExecutor = queryExecutor;
		}

		public void Fill(Dictionary<long, ActivityType> items, Func<ActivityType, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			var query = @"SELECT Id, Name FROM ActivityTypes";

			this.QueryExecutor.Fill(new Query<ActivityType>(query, this.ActivityTypeCreator), items, selector);
		}

		private ActivityType ActivityTypeCreator(IFieldDataReader r)
		{
			var id = r.GetInt64(0);
			var name = string.Empty;
			if (!r.IsDbNull(1))
			{
				name = r.GetString(1);
			}

			return new ActivityType(id, name);
		}
	}

	public sealed class Activity : IDbObject
	{
		public long Id { get; set; }
		public DateTime Date { get; set; }
		public ActivityType ActivityType { get; set; }
		public Visit Visit { get; set; }

		public Activity(long id, DateTime date, ActivityType activityType, Visit visit)
		{
			if (activityType == null) throw new ArgumentNullException(nameof(activityType));
			if (visit == null) throw new ArgumentNullException(nameof(visit));

			this.Id = id;
			this.Date = date;
			this.ActivityType = activityType;
			this.Visit = visit;
		}
	}

	public sealed class ActivityAdapter : IModifiableAdapter<Activity>
	{
		private QueryExecutor QueryExecutor { get; }

		public ActivityAdapter(QueryExecutor queryExecutor)
		{
			if (queryExecutor == null) throw new ArgumentNullException(nameof(queryExecutor));

			this.QueryExecutor = queryExecutor;
		}

		public Task InsertAsync(Activity item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pDate", item.Date),
			new QueryParameter(@"@pActivityTypeId", item.ActivityType.Id),
			new QueryParameter(@"@pVisitId", item.Visit.Id),
		};

			var query = @"INSERT INTO Activities (Date, ActivityTypeId, VisitId) VALUES (@pDate, @pActivityTypeId, @pVisitId)";

			this.QueryExecutor.Execute(query, sqlParams);

			item.Id = this.QueryExecutor.GetNewId();

			return Task.FromResult(true);
		}

		public Task UpdateAsync(Activity item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pId", item.Id),
			new QueryParameter(@"@pDate", item.Date),
			new QueryParameter(@"@pActivityTypeId", item.ActivityType.Id),
			new QueryParameter(@"@pVisitId", item.Visit.Id),
		};

			var query = @"UPDATE Activities SET Date = @pDate, ActivityTypeId = @pActivityTypeId, VisitId = @pVisitId WHERE Id = @pId";

			this.QueryExecutor.Execute(query, sqlParams);

			return Task.FromResult(true);
		}

		public Task DeleteAsync(Activity item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pId", item.Id),
		};

			var query = @"DELETE FROM Activities WHERE Id = @pId";

			this.QueryExecutor.Execute(query, sqlParams);
			return Task.FromResult(true);
		}
	}

	public sealed class Brand
	{
		public long Id { get; }
		public string Name { get; }

		public Brand(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class BrandAdapter : IReadOnlyAdapter<Brand>
	{
		private QueryExecutor QueryExecutor { get; }

		public BrandAdapter(QueryExecutor queryExecutor)
		{
			if (queryExecutor == null) throw new ArgumentNullException(nameof(queryExecutor));

			this.QueryExecutor = queryExecutor;
		}

		public void Fill(Dictionary<long, Brand> items, Func<Brand, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			var query = @"SELECT Id, Name FROM Brands";

			this.QueryExecutor.Fill(new Query<Brand>(query, this.BrandCreator), items, selector);
		}

		private Brand BrandCreator(IFieldDataReader r)
		{
			var id = r.GetInt64(0);
			var name = string.Empty;
			if (!r.IsDbNull(1))
			{
				name = r.GetString(1);
			}

			return new Brand(id, name);
		}
	}

	public sealed class Flavor
	{
		public long Id { get; }
		public string Name { get; }

		public Flavor(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class FlavorAdapter : IReadOnlyAdapter<Flavor>
	{
		private QueryExecutor QueryExecutor { get; }

		public FlavorAdapter(QueryExecutor queryExecutor)
		{
			if (queryExecutor == null) throw new ArgumentNullException(nameof(queryExecutor));

			this.QueryExecutor = queryExecutor;
		}

		public void Fill(Dictionary<long, Flavor> items, Func<Flavor, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			var query = @"SELECT Id, Name FROM Flavors";

			this.QueryExecutor.Fill(new Query<Flavor>(query, this.FlavorCreator), items, selector);
		}

		private Flavor FlavorCreator(IFieldDataReader r)
		{
			var id = r.GetInt64(0);
			var name = string.Empty;
			if (!r.IsDbNull(1))
			{
				name = r.GetString(1);
			}

			return new Flavor(id, name);
		}
	}

	public sealed class Article
	{
		public long Id { get; }
		public string Name { get; }
		public Brand Brand { get; }
		public Flavor Flavor { get; }

		public Article(long id, string name, Brand brand, Flavor flavor)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (brand == null) throw new ArgumentNullException(nameof(brand));
			if (flavor == null) throw new ArgumentNullException(nameof(flavor));

			this.Id = id;
			this.Name = name;
			this.Brand = brand;
			this.Flavor = flavor;
		}
	}

	public sealed class ArticleAdapter : IReadOnlyAdapter<Article>
	{
		private QueryExecutor QueryExecutor { get; }
		private Dictionary<long, Brand> Brands { get; }
		private Dictionary<long, Flavor> Flavors { get; }

		public ArticleAdapter(QueryExecutor queryExecutor, Dictionary<long, Brand> brands, Dictionary<long, Flavor> flavors)
		{
			if (queryExecutor == null) throw new ArgumentNullException(nameof(queryExecutor));
			if (brands == null) throw new ArgumentNullException(nameof(brands));
			if (flavors == null) throw new ArgumentNullException(nameof(flavors));

			this.QueryExecutor = queryExecutor;
			this.Brands = brands;
			this.Flavors = flavors;
		}

		public void Fill(Dictionary<long, Article> items, Func<Article, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			var query = @"SELECT Id, Name, BrandId, FlavorId FROM Articles";

			this.QueryExecutor.Fill(new Query<Article>(query, this.ArticleCreator), items, selector);
		}

		private Article ArticleCreator(IFieldDataReader r)
		{
			var id = r.GetInt64(0);
			var name = string.Empty;
			if (!r.IsDbNull(1))
			{
				name = r.GetString(1);
			}
			var brand = default(Brand);
			if (!r.IsDbNull(2))
			{
				brand = this.Brands[r.GetInt64(2)];
			}
			var flavor = default(Flavor);
			if (!r.IsDbNull(3))
			{
				flavor = this.Flavors[r.GetInt64(3)];
			}

			return new Article(id, name, brand, flavor);
		}
	}

	public sealed class ActivityNoteType
	{
		public long Id { get; }
		public string Name { get; }

		public ActivityNoteType(long id, string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));

			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class ActivityNoteTypeAdapter : IReadOnlyAdapter<ActivityNoteType>
	{
		private QueryExecutor QueryExecutor { get; }

		public ActivityNoteTypeAdapter(QueryExecutor queryExecutor)
		{
			if (queryExecutor == null) throw new ArgumentNullException(nameof(queryExecutor));

			this.QueryExecutor = queryExecutor;
		}

		public void Fill(Dictionary<long, ActivityNoteType> items, Func<ActivityNoteType, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			var query = @"SELECT Id, Name FROM ActivityNoteTypes";

			this.QueryExecutor.Fill(new Query<ActivityNoteType>(query, this.ActivityNoteTypeCreator), items, selector);
		}

		private ActivityNoteType ActivityNoteTypeCreator(IFieldDataReader r)
		{
			var id = r.GetInt64(0);
			var name = string.Empty;
			if (!r.IsDbNull(1))
			{
				name = r.GetString(1);
			}

			return new ActivityNoteType(id, name);
		}
	}

	public sealed class ActivityNote : IDbObject
	{
		public long Id { get; set; }
		public string Contents { get; set; }
		public DateTime CreatedAt { get; set; }
		public ActivityNoteType ActivityNoteType { get; set; }
		public Activity Activity { get; set; }

		public ActivityNote(long id, string contents, DateTime createdAt, ActivityNoteType activityNoteType, Activity activity)
		{
			if (contents == null) throw new ArgumentNullException(nameof(contents));
			if (activityNoteType == null) throw new ArgumentNullException(nameof(activityNoteType));
			if (activity == null) throw new ArgumentNullException(nameof(activity));

			this.Id = id;
			this.Contents = contents;
			this.CreatedAt = createdAt;
			this.ActivityNoteType = activityNoteType;
			this.Activity = activity;
		}
	}

	public sealed class ActivityNoteAdapter : IModifiableAdapter<ActivityNote>
	{
		private QueryExecutor QueryExecutor { get; }
		private Dictionary<long, ActivityNoteType> ActivityNoteTypes { get; }
		private Dictionary<long, Activity> Activities { get; }

		public ActivityNoteAdapter(QueryExecutor queryExecutor, Dictionary<long, ActivityNoteType> activityNoteTypes, Dictionary<long, Activity> activities)
		{
			if (queryExecutor == null) throw new ArgumentNullException(nameof(queryExecutor));
			if (activityNoteTypes == null) throw new ArgumentNullException(nameof(activityNoteTypes));
			if (activities == null) throw new ArgumentNullException(nameof(activities));

			this.QueryExecutor = queryExecutor;
			this.ActivityNoteTypes = activityNoteTypes;
			this.Activities = activities;
		}

		public List<ActivityNote> GetAll()
		{
			var query = @"SELECT Id, Contents, CreatedAt, ActivityNoteTypeId, ActivityId FROM ActivityNotes";

			return this.QueryExecutor.Execute(new Query<ActivityNote>(query, this.ActivityNoteCreator));
		}

		public Task InsertAsync(ActivityNote item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pContents", item.Contents),
			new QueryParameter(@"@pCreatedAt", item.CreatedAt),
			new QueryParameter(@"@pActivityNoteTypeId", item.ActivityNoteType.Id),
			new QueryParameter(@"@pActivityId", item.Activity.Id),
		};

			var query = @"INSERT INTO ActivityNotes (Contents, CreatedAt, ActivityNoteTypeId, ActivityId) VALUES (@pContents, @pCreatedAt, @pActivityNoteTypeId, @pActivityId)";

			this.QueryExecutor.Execute(query, sqlParams);

			item.Id = this.QueryExecutor.GetNewId();
			return Task.FromResult(true);
		}

		public Task UpdateAsync(ActivityNote item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pId", item.Id),
			new QueryParameter(@"@pContents", item.Contents),
			new QueryParameter(@"@pCreatedAt", item.CreatedAt),
			new QueryParameter(@"@pActivityNoteTypeId", item.ActivityNoteType.Id),
			new QueryParameter(@"@pActivityId", item.Activity.Id),
		};

			var query = @"UPDATE ActivityNotes SET Contents = @pContents, CreatedAt = @pCreatedAt, ActivityNoteTypeId = @pActivityNoteTypeId, ActivityId = @pActivityId WHERE Id = @pId";

			this.QueryExecutor.Execute(query, sqlParams);
			return Task.FromResult(true);
		}

		public Task DeleteAsync(ActivityNote item)
		{
			if (item == null) throw new ArgumentNullException(nameof(item));

			var sqlParams = new[]
			{
			new QueryParameter(@"@pId", item.Id),
		};

			var query = @"DELETE FROM ActivityNotes WHERE Id = @pId";

			this.QueryExecutor.Execute(query, sqlParams);
			return Task.FromResult(true);
		}

		private ActivityNote ActivityNoteCreator(IFieldDataReader r)
		{
			var id = r.GetInt64(0);
			var contents = string.Empty;
			if (!r.IsDbNull(1))
			{
				contents = r.GetString(1);
			}
			var createdAt = DateTime.MinValue;
			if (!r.IsDbNull(2))
			{
				createdAt = r.GetDateTime(2);
			}
			var activityNoteType = default(ActivityNoteType);
			if (!r.IsDbNull(3))
			{
				activityNoteType = this.ActivityNoteTypes[r.GetInt64(3)];
			}
			var activity = default(Activity);
			if (!r.IsDbNull(4))
			{
				activity = this.Activities[r.GetInt64(4)];
			}

			return new ActivityNote(id, contents, createdAt, activityNoteType, activity);
		}
	}


	public sealed class OutletViewModel : ViewModel<Outlet>
	{
		public OutletViewModel(Outlet model) : base(model)
		{
		}
	}

	public sealed class OutletsReadOnlyModule : ReadOnlyModule<Outlet, OutletViewModel>
	{
		public OutletsReadOnlyModule(
			Sorter<OutletViewModel> sorter,
			Searcher<OutletViewModel> searcher,
			FilterOption<OutletViewModel>[] filterOptions = null)
			: base(sorter, searcher, filterOptions)
		{
		}
	}


	public sealed class OutletsViewModel : ViewModel
	{
		private Core Core { get; }
		private FeatureManager FeatureManager => this.Core.FeatureManager;
		private ReadOnlyModule<Outlet, OutletViewModel> Module { get; }
		private string Context { get; } = nameof(OutletsViewModel);

		public ObservableCollection<OutletViewModel> Outlets { get; } = new ObservableCollection<OutletViewModel>();
		public SortOption<OutletViewModel>[] SortOptions => this.Module.Sorter.Options;
		public SearchOption<OutletViewModel>[] SearchOptions => this.Module.Searcher.Options;

		private string _textSearch = string.Empty;
		public string TextSearch
		{
			get { return _textSearch; }
			set
			{
				this.SetField(ref _textSearch, value);

				var feature = this.FeatureManager.StartNew(this.Context, nameof(SearchByText));
				this.SearchByText();
				this.FeatureManager.Stop(feature);
			}
		}

		private SearchOption<OutletViewModel> _searchOption;
		public SearchOption<OutletViewModel> SearchOption
		{
			get { return _searchOption; }
			set
			{
				this.SetField(ref _searchOption, value);
				var feature = this.FeatureManager.StartNew(this.Context, nameof(SearchByOption));
				this.SearchByOption();
				this.FeatureManager.Stop(feature, value?.Name ?? string.Empty);
			}
		}

		private SortOption<OutletViewModel> _sortOption;
		public SortOption<OutletViewModel> SortOption
		{
			get { return _sortOption; }
			set
			{
				this.SetField(ref _sortOption, value);
				var feature = this.FeatureManager.StartNew(this.Context, nameof(SortBy));
				this.SortBy();
				this.FeatureManager.Stop(feature);
			}
		}

		public OutletsViewModel(Core core)
		{
			if (core == null) throw new ArgumentNullException(nameof(core));

			this.Core = core;
			var sorter = new Sorter<OutletViewModel>(new[]
			{
			new SortOption<OutletViewModel>(string.Empty, (x, y) => 0),
		});
			var searcher = new Searcher<OutletViewModel>(new[]
			{
			new SearchOption<OutletViewModel>(string.Empty, v => true, true),
		}, (item, search) => true);

			this.Module = new OutletsReadOnlyModule(sorter, searcher);
		}

		public void LoadData()
		{
			var feature = this.FeatureManager.StartNew(this.Context, nameof(LoadData));

			var helper = this.Core.DataCache.GetHelper<Outlet>();
			var viewModels = helper.Items.Values.Select(v => new OutletViewModel(v)).ToArray();
			this.DisplayOutlets(feature, viewModels);

			this.FeatureManager.Stop(feature);
		}

		private void DisplayOutlets(Feature feature, OutletViewModel[] viewModels)
		{
			feature.AddStep(nameof(DisplayOutlets));
			this.Module.SetupViewModels(viewModels);
			this.ApplySearch();
		}

		private void SearchByText() => this.ApplySearch();

		private void SearchByOption() => this.ApplySearch();

		private void ApplySearch()
		{
			var viewModels = this.Module.Search(this.TextSearch, this.SearchOption);

			this.Outlets.Clear();
			foreach (var viewModel in viewModels)
			{
				this.Outlets.Add(viewModel);
			}
		}

		private void SortBy()
		{
			var index = 0;
			foreach (var viewModel in this.Module.Sort(this.Outlets, this.SortOption))
			{
				this.Outlets[index++] = viewModel;
			}
		}
	}

















}