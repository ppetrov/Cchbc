using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;

namespace Cchbc.ConsoleClient
{
	public sealed class Outlet
	{
		public long Id { get; }
		public string Name { get; }

		public Outlet(long id, string name)
		{
			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class Visit
	{
		public long Id { get; }
		public Outlet Outlet { get; }
		public DateTime Date { get; }
		public List<Activity> Activities { get; }

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

	public sealed class ActivityType
	{
		public long Id { get; }
		public string Name { get; }

		public ActivityType(long id, string name)
		{
			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class Activity
	{
		public long Id { get; }
		public DateTime Date { get; }
		public ActivityType ActivityType { get; }
		public Visit Visit { get; }

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

	public sealed class Brand
	{
		public long Id { get; }
		public string Name { get; }

		public Brand(long id, string name)
		{
			this.Id = id;
			this.Name = name;
		}
	}

	public sealed class Flavor
	{
		public long Id { get; }
		public string Name { get; }

		public Flavor(long id, string name)
		{
			this.Id = id;
			this.Name = name;
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
			if (brand == null) throw new ArgumentNullException(nameof(brand));
			if (flavor == null) throw new ArgumentNullException(nameof(flavor));

			this.Id = id;
			this.Name = name;
			this.Brand = brand;
			this.Flavor = flavor;
		}
	}





	public sealed class OutletAdapter : IReadOnlyAdapter<Outlet>
	{
		private readonly ReadQueryHelper _queryHelper;

		public OutletAdapter(ReadQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			_queryHelper = queryHelper;
		}

		public Task FillAsync(Dictionary<long, Outlet> items, Func<Outlet, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			return _queryHelper.FillAsync(new Query<Outlet>(@"SELECT Id, Name FROM Outlets", r =>
			{
				var id = 0L;
				if (!r.IsDbNull(0))
				{
					id = r.GetInt64(0);
				}

				var name = string.Empty;
				if (!r.IsDbNull(1))
				{
					name = r.GetString(1);
				}

				return new Outlet(id, name);
			}), items, selector);
		}
	}

	public sealed class ActivityTypeAdapter : IReadOnlyAdapter<ActivityType>
	{
		private readonly ReadQueryHelper _queryHelper;

		public ActivityTypeAdapter(ReadQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			_queryHelper = queryHelper;
		}

		public Task FillAsync(Dictionary<long, ActivityType> items, Func<ActivityType, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			return _queryHelper.FillAsync(new Query<ActivityType>(@"SELECT Id, Name FROM ActivityTypes", r =>
			{
				var id = 0L;
				if (!r.IsDbNull(0))
				{
					id = r.GetInt64(0);
				}

				var name = string.Empty;
				if (!r.IsDbNull(1))
				{
					name = r.GetString(1);
				}

				return new ActivityType(id, name);
			}), items, selector);
		}
	}

	public sealed class BrandAdapter : IReadOnlyAdapter<Brand>
	{
		private readonly ReadQueryHelper _queryHelper;

		public BrandAdapter(ReadQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			_queryHelper = queryHelper;
		}

		public Task FillAsync(Dictionary<long, Brand> items, Func<Brand, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			return _queryHelper.FillAsync(new Query<Brand>(@"SELECT Id, Name FROM Brands", r =>
			{
				var id = 0L;
				if (!r.IsDbNull(0))
				{
					id = r.GetInt64(0);
				}

				var name = string.Empty;
				if (!r.IsDbNull(1))
				{
					name = r.GetString(1);
				}

				return new Brand(id, name);
			}), items, selector);
		}
	}

	public sealed class FlavorAdapter : IReadOnlyAdapter<Flavor>
	{
		private readonly ReadQueryHelper _queryHelper;

		public FlavorAdapter(ReadQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			_queryHelper = queryHelper;
		}

		public Task FillAsync(Dictionary<long, Flavor> items, Func<Flavor, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			return _queryHelper.FillAsync(new Query<Flavor>(@"SELECT Id, Name FROM Flavors", r =>
			{
				var id = 0L;
				if (!r.IsDbNull(0))
				{
					id = r.GetInt64(0);
				}

				var name = string.Empty;
				if (!r.IsDbNull(1))
				{
					name = r.GetString(1);
				}

				return new Flavor(id, name);
			}), items, selector);
		}
	}

	public sealed class ArticleAdapter : IReadOnlyAdapter<Article>
	{
		private readonly ReadQueryHelper _queryHelper;
		private readonly Dictionary<long, Brand> _brands;
		private readonly Dictionary<long, Flavor> _flavors;

		public ArticleAdapter(ReadQueryHelper queryHelper, Dictionary<long, Brand> brands, Dictionary<long, Flavor> flavors)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));
			if (brands == null) throw new ArgumentNullException(nameof(brands));
			if (flavors == null) throw new ArgumentNullException(nameof(flavors));

			_queryHelper = queryHelper;
			_brands = brands;
			_flavors = flavors;
		}

		public Task FillAsync(Dictionary<long, Article> items, Func<Article, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			return _queryHelper.FillAsync(new Query<Article>(@"SELECT Id, Name, BrandId, FlavorId FROM Articles", r =>
			{
				var id = 0L;
				if (!r.IsDbNull(0))
				{
					id = r.GetInt64(0);
				}

				var name = string.Empty;
				if (!r.IsDbNull(1))
				{
					name = r.GetString(1);
				}

				var brand = default(Brand);
				if (!r.IsDbNull(2))
				{
					brand = _brands[r.GetInt64(2)];
				}

				var flavor = default(Flavor);
				if (!r.IsDbNull(3))
				{
					flavor = _flavors[r.GetInt64(3)];
				}

				return new Article(id, name, brand, flavor);
			}), items, selector);
		}
	}






}