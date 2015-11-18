using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cchbc.Data;
using Cchbc.Objects;

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









	public sealed class OutletAdapter : IReadOnlyAdapter<Outlet>
	{
		private ReadQueryHelper QueryHelper { get; }

		public OutletAdapter(ReadQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.QueryHelper = queryHelper;
		}

		public Task FillAsync(Dictionary<long, Outlet> items, Func<Outlet, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			return this.QueryHelper.FillAsync(new Query<Outlet>(@"SELECT Id, Name FROM Outlets", r =>
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
		private ReadQueryHelper QueryHelper { get; }
		private Dictionary<long, Brand> Brands { get; }
		private Dictionary<long, Flavor> Flavors { get; }

		public ArticleAdapter(ReadQueryHelper queryHelper, Dictionary<long, Brand> brands, Dictionary<long, Flavor> flavors)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));
			if (brands == null) throw new ArgumentNullException(nameof(brands));
			if (flavors == null) throw new ArgumentNullException(nameof(flavors));

			this.QueryHelper = queryHelper;
			this.Brands = brands;
			this.Flavors = flavors;
		}

		public Task FillAsync(Dictionary<long, Article> items, Func<Article, long> selector)
		{
			if (items == null) throw new ArgumentNullException(nameof(items));
			if (selector == null) throw new ArgumentNullException(nameof(selector));

			return this.QueryHelper.FillAsync(new Query<Article>(@"SELECT Id, Name, BrandsId, FlavorsId FROM Articles", r =>
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
					brand = this.Brands[r.GetInt64(2)];
				}

				var flavor = default(Flavor);
				if (!r.IsDbNull(3))
				{
					flavor = this.Flavors[r.GetInt64(3)];
				}

				return new Article(id, name, brand, flavor);
			}), items, selector);
		}
	}




	public sealed class VisitAdapter : IModifiableAdapter<Visit>
	{
		private ReadQueryHelper QueryHelper { get; }
		private Dictionary<long, Outlet> Outlets { get; }

		public VisitAdapter(ReadQueryHelper queryHelper, Dictionary<long, Outlet> outlets)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));
			if (outlets == null) throw new ArgumentNullException(nameof(outlets));

			this.QueryHelper = queryHelper;
			this.Outlets = outlets;
		}

		//GET ALL !!!


		public Task InsertAsync(Visit item)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(Visit item)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(Visit item)
		{
			throw new NotImplementedException();
		}

	}


	public sealed class ActivityAdapter : IModifiableAdapter<Activity>
	{
		private ReadQueryHelper QueryHelper { get; }

		public ActivityAdapter(ReadQueryHelper queryHelper)
		{
			if (queryHelper == null) throw new ArgumentNullException(nameof(queryHelper));

			this.QueryHelper = queryHelper;
		}

		public Task InsertAsync(Activity item)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(Activity item)
		{
			throw new NotImplementedException();
		}

		public Task DeleteAsync(Activity item)
		{
			throw new NotImplementedException();
		}

	}







}