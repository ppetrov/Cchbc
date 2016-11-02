using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cchbc.Data;
using Cchbc.Dialog;
using Cchbc.Features;
using Cchbc.Objects;

namespace ConsoleClient
{
	public sealed class AddActivityViewModelData : IAddActivityViewModelData
	{
		private sealed class TypeRow
		{
			public readonly long CategoryId;
			public readonly long Id;
			public readonly string Name;

			public TypeRow(long categoryId, long id, string name)
			{
				CategoryId = categoryId;
				Id = id;
				Name = name;
			}
		}

		private sealed class CategoryRow
		{
			public readonly long Id;
			public readonly string Name;
			public readonly long AutoTypeId;

			public CategoryRow(long id, string name, long autoTypeId)
			{
				Id = id;
				Name = name;
				AutoTypeId = autoTypeId;
			}
		}

		private ITransactionContext Context { get; }

		public List<Outlet> Outlets { get; } = new List<Outlet>();
		public List<ActivityTypeCategory> Categories { get; } = new List<ActivityTypeCategory>();
		public bool WithVisit { get; private set; }

		public AddActivityViewModelData(ITransactionContext context)
		{
			if (context == null) throw new ArgumentNullException(nameof(context));

			this.Context = context;
		}

		public void Load()
		{
			// We only need a data context and probably a cache
			// How we can integrate a cache ?????
			var outlets = this.Context.Execute(new Query<Outlet>(@"select * from outlets", r => null));

			var typeRows = this.Context.Execute(new Query<TypeRow>(@"select * from activity_types", r => new TypeRow(0, 0, string.Empty)));
			var categoryRows = this.Context.Execute(new Query<CategoryRow>(@"select * from activity_categories", r => new CategoryRow(0, string.Empty, 0)));

			foreach (var category in categoryRows)
			{
				var categoryId = category.Id;
				var byCategory = GetTypesByCategory(typeRows, categoryId);
				var autoType = GetType(byCategory, category.AutoTypeId);

				this.Categories.Add(new ActivityTypeCategory(categoryId, category.Name, byCategory.ToArray(), autoType));
			}

			// TODO : !!!
			this.WithVisit = false;
		}

		private static List<ActivityType> GetTypesByCategory(IEnumerable<TypeRow> types, long categoryId)
		{
			var byCategory = new List<ActivityType>();

			foreach (var type in types)
			{
				if (type.CategoryId == categoryId)
				{
					byCategory.Add(new ActivityType(type.Id, type.Name));
				}
			}

			return byCategory;
		}

		private static ActivityType GetType(IEnumerable<ActivityType> types, long typeId)
		{
			foreach (var type in types)
			{
				if (type.Id == typeId)
				{
					return type;
				}
			}
			return null;
		}
	}

	public sealed class DateRange
	{
		public DateTime FromDate { get; }
		public DateTime ToDate { get; }

		public DateRange(DateTime fromDate, DateTime toDate)
		{
			this.FromDate = fromDate;
			this.ToDate = toDate;
		}
	}

	public interface IActivityCreator
	{
		Activity Create(Outlet outlet, ActivityType activityType, DateTime fromDate, DateTime toDate, bool withVisit);
	}

	public sealed class ActivityCreator : IActivityCreator
	{
		public Activity Create(Outlet outlet, ActivityType activityType, DateTime fromDate, DateTime toDate, bool withVisit)
		{
			throw new NotImplementedException();
		}
	}

	public sealed class AddActivityViewModel : ViewModel
	{
		public ObservableCollection<Outlet> Outlets { get; } = new ObservableCollection<Outlet>();
		public ObservableCollection<ActivityTypeCategory> Categories { get; } = new ObservableCollection<ActivityTypeCategory>();
		public ObservableCollection<ActivityType> Types { get; } = new ObservableCollection<ActivityType>();
		public AppUsageManager AppUsageManager { get; } = new AppUsageManager(new AppUsageAdapter());
		public IModalDialog ModalDialog { get; }
		public IActivityCreator ActivityCreator { get; }

		public AddActivityViewModel(IActivityCreator activityCreator, IModalDialog modalDialog)
		{
			if (activityCreator == null) throw new ArgumentNullException(nameof(activityCreator));
			if (modalDialog == null) throw new ArgumentNullException(nameof(modalDialog));

			this.ActivityCreator = activityCreator;
			this.ModalDialog = modalDialog;
		}

		private Outlet _selectedOutlet;
		public Outlet SelectedOutlet
		{
			get { return _selectedOutlet; }
			set
			{
				this.AppUsageManager.RecordButton(AppUsageContext.AddActivity, AppUsageOperation.SelectOutlet);
				this.SetProperty(out _selectedOutlet, value);
			}
		}

		private ActivityTypeCategory _selectedCategory;
		public ActivityTypeCategory SelectedCategory
		{
			get { return _selectedCategory; }
			set
			{
				this.AppUsageManager.RecordButton(AppUsageContext.AddActivity, AppUsageOperation.SelectCategory);
				this.SetProperty(out _selectedCategory, value);

				this.Types.Clear();
				if (this.SelectedCategory != null)
				{
					foreach (var type in this.SelectedCategory.Types)
					{
						this.Types.Add(type);
					}
					this.SelectedType = this.SelectedCategory.AutoSelectedType;
				}
			}
		}

		private ActivityType _selectedType;
		public ActivityType SelectedType
		{
			get { return _selectedType; }
			set
			{
				this.AppUsageManager.RecordButton(AppUsageContext.AddActivity, AppUsageOperation.SelectType);
				this.SetProperty(out _selectedType, value);
			}
		}

		private DateTime _fromDate;
		public DateTime FromDate
		{
			get { return _fromDate; }
			set
			{
				this.AppUsageManager.RecordDateTimePicker(AppUsageContext.AddActivity, AppUsageOperation.SelectFromDate);
				this.SetProperty(out _fromDate, value);
			}
		}

		private DateTime _toDate;
		public DateTime ToDate
		{
			get { return _toDate; }
			set
			{
				this.AppUsageManager.RecordDateTimePicker(AppUsageContext.AddActivity, AppUsageOperation.SelectFromDate);
				this.SetProperty(out _toDate, value);
			}
		}

		private bool _withVisit;
		public bool WithVisit
		{
			get { return _withVisit; }
			set
			{
				this.AppUsageManager.RecordCheckBox(AppUsageContext.AddActivity, AppUsageOperation.ToggleVisit);
				this.SetProperty(out _withVisit, value);
			}
		}

		public void Load(AddActivityViewModelData data)
		{
			if (data == null) throw new ArgumentNullException(nameof(data));

			this.FromDate = this.ToDate = DateTime.Now;

			this.Outlets.Clear();
			foreach (var outlet in data.Outlets)
			{
				this.Outlets.Add(outlet);
			}

			this.Categories.Clear();
			foreach (var category in data.Categories)
			{
				this.Categories.Add(category);
			}

			if (this.Categories.Count > 0)
			{
				this.SelectedCategory = this.Categories[0];
			}
			this.WithVisit = data.WithVisit;
		}

		public Activity CreateActivity()
		{
			this.AppUsageManager.RecordButton(AppUsageContext.AddActivity, AppUsageOperation.Create);
			return this.CreateActivityImpl();
		}

		public Activity StartActivity()
		{
			this.AppUsageManager.RecordButton(AppUsageContext.AddActivity, AppUsageOperation.Start);
			return this.CreateActivityImpl();
		}

		private Activity CreateActivityImpl()
		{
			if (this.SelectedOutlet == null)
			{
				this.ModalDialog.ShowAsync(@"TODO:", Feature.None);
				return null;
			}
			if (this.SelectedType == null)
			{
				this.ModalDialog.ShowAsync(@"TODO:", Feature.None);
				return null;
			}
			if (this.FromDate > this.ToDate)
			{
				this.ModalDialog.ShowAsync(@"TODO:", Feature.None);
				return null;
			}
			return this.ActivityCreator.Create(this.SelectedOutlet, this.SelectedType, this.FromDate, this.ToDate, this.WithVisit);
		}
	}

	public sealed class Activity
	{
		public long Id { get; set; }
		public string Name { get; }
		public ActivityType Type { get; }
		public string Details { get; set; } = string.Empty;
		public DateTime FromDate { get; set; } = DateTime.MinValue;
		public DateTime ToDate { get; set; } = DateTime.MaxValue;

		public Activity(long id, string name, ActivityType type)
		{
			this.Id = id;
			this.Name = name;
			this.Type = type;
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

	public sealed class ActivityTypeCategory
	{
		public long Id { get; }
		public string Name { get; }
		public ActivityType[] Types { get; }
		public ActivityType AutoSelectedType { get; }

		public ActivityTypeCategory(long id, string name, ActivityType[] types, ActivityType autoSelectedType = null)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (types == null) throw new ArgumentNullException(nameof(types));

			this.Id = id;
			this.Name = name;
			this.Types = types;
			this.AutoSelectedType = autoSelectedType;
		}
	}

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
}