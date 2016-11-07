using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Cchbc;
using Cchbc.Common;
using Cchbc.Data;
using Cchbc.Logs;
using Cchbc.Objects;

namespace ConsoleClient
{
	public sealed class AgendaViewModelData
	{
		private IDbContext Context { get; }
		private DataCache Cache { get; }

		public List<AgendaOutlet> Outlets { get; } = new List<AgendaOutlet>();

		public AgendaViewModelData(IDbContext context, DataCache cache)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.Context = context;
			this.Cache = cache;
		}

		public void Load(DateTime date)
		{
			//var outlets = this.Cache.GetValues<Article>(this.Context);
			Console.WriteLine(@"select * from outlets");
			Console.WriteLine(@"select * from activities");

			//var outlets = this.Context.Execute(new Query<Outlet>(@"select * from outlets where ???", r => null));

			// Query visits for the date
			// Query for the date
			//var activties = this.Context.Execute(new Query<Activity>(@"select * from activities", r => null));
		}
	}

	public sealed class Brand
	{
		public long Id { get; }
		public string Name { get; }
	}

	public sealed class Flavor
	{
		public long Id { get; }
		public string Name { get; }
	}

	public sealed class Article
	{
		public long Id { get; }
		public string Name { get; }
		public Brand Brand { get; set; }
		public Flavor Flavor { get; set; }
	}


	public static class DataProvider
	{
		public static Dictionary<long, Brand> GetBrands(IDbContext context, DataCache cache)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (cache == null) throw new ArgumentNullException(nameof(cache));

			var outlets = context.Execute(new Query<Brand>(string.Empty, r => null));

			return null;
		}

		public static Dictionary<long, Flavor> GetFlavors(IDbContext context, DataCache cache)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (cache == null) throw new ArgumentNullException(nameof(cache));

			var outlets = context.Execute(new Query<Flavor>(string.Empty, r => null));

			return null;
		}

		public static Dictionary<long, Article> GetArticles(IDbContext context, DataCache cache)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (cache == null) throw new ArgumentNullException(nameof(cache));

			var brands = cache.GetValues<Brand>(context);
			var flavors = cache.GetValues<Flavor>(context);
			var outlets = context.Execute(new Query<Article>(string.Empty, r => new Article()
			{
				Brand = brands[0],
				Flavor = flavors[0]
			}));

			cache.RemoveValues<Brand>();
			cache.RemoveValues<Flavor>();

			return null;
		}

		public static Dictionary<long, ActivityTypeCategory> GetActivityTypeCategories(IDbContext context, DataCache cache)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			// We don't need anything
			var outlets = context.Execute(new Query<ActivityTypeCategory>(string.Empty, r => null));

			return null;
		}

		public static Dictionary<long, Outlet> GetOutlets(IDbContext context, DataCache cache)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));
			if (cache == null) throw new ArgumentNullException(nameof(cache));

			// We don't need anything
			var outlets = context.Execute(new Query<Outlet>(@"select * from outlets where ???", r => null));

			return null;
		}
	}




	public sealed class AgendaViewModel : ViewModel
	{
		public DateTime CurrentDate { get; private set; }
		public AppContext AppContext { get; }

		public ICommand GoToNextDay { get; }
		public ICommand GoToPreviousDay { get; }
		public ICommand CloseActivities { get; }

		public ObservableCollection<AgendaOutletViewModel> Outlets { get; } = new ObservableCollection<AgendaOutletViewModel>();

		private AgendaOutletViewModel _selectedOutlet;
		public AgendaOutletViewModel SelectedOutlet
		{
			get { return _selectedOutlet; }
			set
			{
				this.SetProperty(out _selectedOutlet, value);
			}
		}

		public Func<string> CloseReasonSelector { get; }

		public AgendaViewModel(AppContext appContext)
		{
			if (appContext == null) throw new ArgumentNullException(nameof(appContext));

			this.AppContext = appContext;
			this.CurrentDate = DateTime.Today;
			this.GoToNextDay = new RelayCommand(() =>
			{
				this.ChangeDate(this.CurrentDate.AddDays(1));
			});
			this.GoToPreviousDay = new RelayCommand(() =>
			{
				this.ChangeDate(this.CurrentDate.AddDays(-1));
			});
			this.CloseActivities = new RelayCommand(() =>
			{
				if (this.SelectedOutlet == null) return;

				var closeReason = this.CloseReasonSelector();
				if (closeReason == null) return;

				foreach (var activity in this.SelectedOutlet.Activities)
				{
					//activity.Close = true;
				}
			});
		}

		public void Load()
		{
			try
			{
				using (var ctx = this.AppContext.DbContextCreator())
				{
					var data = new AgendaViewModelData(ctx, this.AppContext.DataCache);
					data.Load(this.CurrentDate);

					this.Outlets.Clear();
					foreach (var outlet in data.Outlets)
					{
						this.Outlets.Add(new AgendaOutletViewModel(outlet));
					}

					ctx.Complete();
				}
			}
			catch (Exception ex)
			{
				this.AppContext.Log(ex.ToString(), LogLevel.Error);
			}
		}

		private void ChangeDate(DateTime date)
		{
			this.CurrentDate = date;
			this.Load();
		}
	}

	public sealed class ActivityViewModel : ViewModel<Activity>
	{
		public string Type { get; }
		public DateTime FromDate { get; }
		public DateTime ToDate { get; }

		public ActivityViewModel(Activity model) : base(model)
		{
			this.Type = model.Type.Name;
			this.FromDate = model.FromDate;
			this.ToDate = model.ToDate;
		}
	}


	public sealed class AgendaOutlet
	{
		public Outlet Outlet { get; }
		public List<Activity> Activities { get; } = new List<Activity>();

		public AgendaOutlet(Outlet outlet)
		{
			if (outlet == null) throw new ArgumentNullException(nameof(outlet));

			this.Outlet = outlet;
		}
	}

	public sealed class AgendaOutletViewModel : ViewModel<AgendaOutlet>
	{
		public string OutletNumber { get; }
		public string OutletName { get; }
		public ObservableCollection<ActivityViewModel> Activities { get; } = new ObservableCollection<ActivityViewModel>();

		public AgendaOutletViewModel(AgendaOutlet model) : base(model)
		{
			var outlet = model.Outlet;
			this.OutletNumber = outlet.Id.ToString();
			this.OutletName = outlet.Name;
		}
	}


}